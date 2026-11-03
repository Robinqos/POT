using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;

namespace FileDetective.Desktop;

// Spoločná hotová infraštruktúra: odozva, chyby, životnosť a zatvorenie.
public abstract class ExperimentWindow : Window
{
    protected readonly StackPanel Panel = new() { Margin = new Thickness(24), Spacing = 16 };
    private readonly TextBlock _status = new() { Text = "Pripravené" };
    private readonly TextBlock _heartbeat = new() { Text = "Čas: 0,0 s" };
    protected readonly TextBox Result = new() { IsReadOnly = true, AcceptsReturn = true,
        TextWrapping = Avalonia.Media.TextWrapping.Wrap, MinHeight = 120 };
    private readonly Button _cancel = new() { Content = "Zrušiť operáciu", IsEnabled = false };
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private readonly List<Button> _starts = [];
    private CancellationTokenSource? _active;
    private Task? _work;
    private bool _closing;
    private bool _allowClose;
    private readonly Stopwatch _clock = new();

    protected ExperimentWindow(string title, string description)
    {
        Title = "Lab05 | " + title;
        Width = 760; Height = 660; MinWidth = 700; MinHeight = 600;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Panel.Children.Add(new TextBlock { Text = title, FontSize = 24 });
        Panel.Children.Add(new TextBlock { Text = description, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        Panel.Children.Add(_heartbeat);
        _cancel.Click += (_, _) => _active?.Cancel();
        _timer.Tick += (_, _) => { if (_active != null) _heartbeat.Text = $"Čas: {_clock.Elapsed.TotalSeconds:F1} s"; };
        _timer.Start();
        Closing += OnClosing;
        Closed += (_, _) => _timer.Stop();
    }

    protected void AddStart(string label, Func<CancellationToken, Task> body, params Control[] extra)
    {
        var start = new Button { Content = label };
        // async void iba na hranici udalosti; RunUiAsync zachytí chyby operácie.
        start.Click += async (_, _) => await RunUiAsync(body);
        _starts.Add(start);
        var controls = new List<Control> { start };
        controls.AddRange(extra);
        if (_starts.Count == 1) controls.Add(_cancel);
        Panel.Children.Add(Row(controls.ToArray()));
    }

    protected void FinishLayout()
    {
        Panel.Children.Add(_status); Panel.Children.Add(Result);
        Content = new ScrollViewer { Content = Panel };
    }

    protected static StackPanel Row(params Control[] children)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        foreach (var child in children)
        {
            child.VerticalAlignment = VerticalAlignment.Center;
            row.Children.Add(child);
        }
        return row;
    }

    protected void Log(string message) => Result.Text = message + Environment.NewLine + Result.Text;
    protected virtual void ResetOperation() { }
    protected bool IsBusy => _active != null;

    protected async Task RunUiAsync(Func<CancellationToken, Task> body)
    {
        if (_active != null || _closing) return;
        using var source = new CancellationTokenSource();
        _active = source;
        foreach (var button in _starts) button.IsEnabled = false;
        _cancel.IsEnabled = true;
        _status.Text = "Prebieha operácia";
        _clock.Restart();
        try
        {
            _work = body(source.Token);
            await _work;
            _status.Text = $"Hotovo za {_clock.ElapsedMilliseconds} ms";
        }
        catch (OperationCanceledException) when (source.IsCancellationRequested)
        { _status.Text = "Zrušené používateľom"; }
        catch (TimeoutException) { _status.Text = "Časový limit auditu"; }
        catch (Exception ex) { _status.Text = "Chyba"; Log(ex.Message); }
        finally
        {
            _active = null; _work = null; _clock.Stop();
            _cancel.IsEnabled = false;
            ResetOperation();
            foreach (var button in _starts) button.IsEnabled = !_closing;
        }
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose || _active == null) return;
        e.Cancel = true;
        if (_closing) return;
        _closing = true;
        _active.Cancel();
        var pending = _work;
        try { if (pending != null) await pending; }
        catch (Exception ex) { Log($"Ukončenie operácie: {ex.Message}"); }
        finally { _allowClose = true; Close(); }
    }
}
