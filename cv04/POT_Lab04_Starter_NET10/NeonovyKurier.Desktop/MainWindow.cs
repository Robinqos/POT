using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NeonovyKurier.Core;
using NeonovyKurier.Cli;
using NeonovyKurier.Cli.Infrastructure;

namespace NeonovyKurier.Desktop;

public sealed partial class MainWindow : Window
{
    private HttpClient? _client;
    private DemoServer? _server;
    private readonly bool _ownsClient;
    private readonly string _directory;
    private CancellationTokenSource? _cancel;
    private Task _startup = Task.CompletedTask, _operation = Task.CompletedTask;
    private Task? _shutdown;
    private bool _busy, _closing, _allowClose;
    private int _responseCount, _ticks, _session;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(100) };

    public MainWindow() : this(null, null) { }
    public MainWindow(HttpClient? client, string? directory)
    {
        _client = client; _ownsClient = client is null;
        _directory = directory ?? MissionContent.InstalledDirectory;
        BuildLayout(); SetBusy(false);
        _timer.Tick += (_, _) => _pulse.Text = $"UI žije: {++_ticks}";
        _timer.Start();
        Opened += WindowOpened; Closing += WindowClosing;
        if (client is not null) _status.Text = "Pripravené";
    }

    private async void WindowOpened(object? sender, EventArgs e)
    {
        _startup = InitializeAsync();
        try { await _startup; } catch (Exception error) { _status.Text = error.Message; }
    }
    private async Task InitializeAsync()
    {
        if (_client is not null) return;
        try
        {
            _server = await DemoServer.StartAsync();
            _client = new HttpClient { BaseAddress = _server.BaseAddress };
            if (!_closing) { _status.Text = "Pripravené"; SetBusy(false); }
        }
        catch (Exception error) { if (!_closing) _status.Text = $"Server: {error.Message}"; }
    }

    // Async void je iba hranica UI udalosti; vlastná operácia vracia Task.
    private async void InstallClicked(object? sender, RoutedEventArgs e)
    {
        try { await RunDownloadAsync(); } catch (Exception error) { _status.Text = error.Message; }
    }
    private async void OneClicked(object? sender, RoutedEventArgs e)
    {
        try { await RunDownloadAsync(onlyLevel: true); } catch (Exception error) { _status.Text = error.Message; }
    }
    private async void DownloadClicked(object? sender, RoutedEventArgs e)
    {
        try { await RunDownloadAsync(downloadOnly: true); } catch (Exception error) { _status.Text = error.Message; }
    }
    public Task RunDownloadAsync(bool onlyLevel = false, bool downloadOnly = false)
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_busy) return _operation;
        if (_closing || _client is null) return Task.CompletedTask;
        SetBusy(true); _cancel = new CancellationTokenSource();
        int session = ++_session;
        _operation = RunDownloadCoreAsync(onlyLevel, downloadOnly, session, _cancel.Token);
        return _operation;
    }

    private async Task RunDownloadCoreAsync(bool onlyLevel, bool downloadOnly, int session, CancellationToken token)
    {
        var clock = Stopwatch.StartNew(); var completed = new HashSet<string>();
        foreach (var state in _states.Values) state.Text = "čaká";
        _progressBar.Value = 0; _summary.Text = "0 z 3 súborov"; _log.Text = "";
        _status.Text = "Sťahujem...";
        // Vytvorené na UI vlákne: Report sa vracia do jeho SynchronizationContext.
        var progress = new Progress<DownloadProgress>(value =>
        {
            if (session != _session || _closing || !_busy) return;
            _states[value.AssetId].Text = value.Stage;
            if (value.Stage is "DONE" or "CACHE") completed.Add(value.AssetId);
            _progressBar.Value = completed.Count; _summary.Text = $"{completed.Count} z 3 súborov";
            _log.Text += $"{clock.ElapsedMilliseconds,5} ms  {value.Stage,-5} {value.AssetId}{Environment.NewLine}";
        });
        var service = new DownloadService(_client!, progress);
        GameAsset[] assets = (_failure.IsChecked == true ? MissionContent.WithFailure : MissionContent.Assets);
        if (_slow.IsChecked == true) assets = assets.Select(a => a.Route == "/failure" ? a : a with { Route = "/slow" + a.Route }).ToArray();
        int mode = _mode.SelectedIndex; // Nastavenia si zapamätáme pred prvým await.
        try
        {
            DownloadResult[] results;
            if (onlyLevel) results = [await service.DownloadAssetAsync(assets[0], token)];
            else if (mode == 0) results = await service.DownloadSequentialAsync(assets, token);
            else if (mode == 1) results = await service.DownloadConcurrentAsync(assets, token);
            else if (mode == 3) results = await service.DownloadLimitedAsync(assets, 2, token);
            else
            {
                var tasks = new List<Task<DownloadResult>>();
                foreach (GameAsset asset in assets) tasks.Add(service.DownloadWithCacheAsync(asset, _directory, token));
                results = await Task.WhenAll(tasks);
            }
            if (!onlyLevel && !downloadOnly)
            {
                _status.Text = "Ukladám balíček...";
                MissionPackage.Invalidate(_directory);
                await service.SavePackAsync(_directory, results, token);
                await MissionPackage.MarkReadyAsync(_directory, token);
            }
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            foreach (var result in results) _states[result.Asset.Id].Text = result.FromCache ? "CACHE" : "DONE";
            _progressBar.Value = results.Length; _summary.Text = $"{results.Length} z 3 súborov";
            string action = onlyLevel ? "Mapa získaná" : downloadOnly ? "Údaje stiahnuté" : "Misia pripravená";
            _status.Text = $"{action} za {clock.ElapsedMilliseconds} ms";
            _log.Text += $"{_status.Text}{Environment.NewLine}{(onlyLevel ? results[0].Content : downloadOnly ? "Obsah zatiaľ zostáva v pamäti." : _directory)}";
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { _status.Text = "Zrušené používateľom"; }
        catch (NotImplementedException error) { _status.Text = $"Doplňte úlohu {error.Message}"; }
        catch (HttpRequestException error) { _status.Text = $"HTTP chyba: {error.StatusCode}. Po prvom úspešnom stiahnutí skúste režim s cache."; }
        catch (Exception error) { _status.Text = $"Chyba: {error.Message}"; }
        finally
        {
            // Dočkáme už odoslané hlásenia Progress, vrátane posledného DONE.
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            _cancel?.Dispose(); _cancel = null;
            SetBusy(false); // Obnovenie stavu aj pri chybe alebo zrušení.
        }
    }

    private void SetBusy(bool value)
    {
        _busy = value; bool enabled = !value && !_closing && _client is not null;
        _install.IsEnabled = _one.IsEnabled = _download.IsEnabled = enabled;
        _mode.IsEnabled = _failure.IsEnabled = _slow.IsEnabled = _training.IsEnabled = !value && !_closing;
        bool ready = MissionPackage.IsReady(_directory);
        _play.IsEnabled = ready && !value && !_closing;
        _missionState.Text = ready
            ? "Hlavná misia je pripravená. Pri aktualizácii môžeš hrať v už otvorenom okne."
            : "Hlavná misia je zamknutá. Doplň U2-U4 a stiahni celý balíček. Tréning je dostupný.";
        _cancelButton.IsEnabled = value && !_closing;
    }

    private async void AwaitProbeClicked(object? sender, RoutedEventArgs e)
    {
        try { await RunAwaitProbeAsync(); } catch (Exception error) { _status.Text = error.Message; }
    }
    public Task RunAwaitProbeAsync()
    {
        if (_busy) return _operation;
        if (_closing) return Task.CompletedTask;
        SetBusy(true); _cancel = new CancellationTokenSource();
        _operation = AwaitProbeCoreAsync(_cancel.Token);
        return _operation;
    }
    private async Task AwaitProbeCoreAsync(CancellationToken token)
    {
        try
        {
            _status.Text = "Čakám cez await. Skúste odozvu a hru.";
            _log.Text = $"Pred await: thread={Environment.CurrentManagedThreadId}, SC={SynchronizationContext.Current?.GetType().Name}";
            // U1 UI: krátky hotový pokus, porovnajte s tlačidlom Sleep.
            await Task.Delay(1500, token);
            _status.Text = "Await dokončený; UI mohlo reagovať.";
            _log.Text += $"{Environment.NewLine}Po await: thread={Environment.CurrentManagedThreadId}, SC={SynchronizationContext.Current?.GetType().Name}";
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { _status.Text = "Pokus zrušený"; }
        finally { _cancel?.Dispose(); _cancel = null; SetBusy(false); }
    }

    public Task ShutdownAsync() => _shutdown ??= ShutdownCoreAsync();
    private async Task ShutdownCoreAsync()
    {
        _closing = true; ++_session; _timer.Stop(); _cancel?.Cancel();
        ActiveGame?.Close();
        await _startup; await _operation;
        if (_ownsClient) _client?.Dispose();
        if (_server is not null) await _server.DisposeAsync();
    }
    private async void WindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose) return;
        e.Cancel = true;
        if (_closing) return;
        try { await ShutdownAsync(); }
        catch (Exception error) { Trace.WriteLine(error); }
        finally { _allowClose = true; Close(); }
    }
}
