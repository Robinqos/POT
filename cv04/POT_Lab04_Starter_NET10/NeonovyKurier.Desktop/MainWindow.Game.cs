using Avalonia.Interactivity;
using Avalonia.Threading;
using NeonovyKurier.Core;

namespace NeonovyKurier.Desktop;

public sealed partial class MainWindow
{
    public GameWindow? ActiveGame { get; private set; }

    private async void PlayClicked(object? sender, RoutedEventArgs e)
    {
        try { await LoadGameAsync(); } catch (Exception error) { _status.Text = error.Message; }
    }

    private GameWindow ShowGame()
    {
        if (ActiveGame is null)
        {
            ActiveGame = new GameWindow();
            ActiveGame.Closed += (_, _) => ActiveGame = null;
            ActiveGame.Show(this);
        }
        ActiveGame.Activate();
        return ActiveGame;
    }

    public void ShowTraining()
    {
        if (_busy || _closing) return;
        ShowGame().LoadMission(MissionDefinition.Training, "@",
            "Toto je iba tréning. Hlavná misia potrebuje úspešné stiahnutie a uloženie dát.", isTraining: true);
    }

    public Task LoadGameAsync()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_busy) return _operation;
        if (_closing) return Task.CompletedTask;
        SetBusy(true); _cancel = new CancellationTokenSource();
        _operation = LoadGameCoreAsync(_cancel.Token);
        return _operation;
    }

    private async Task LoadGameCoreAsync(CancellationToken token)
    {
        try
        {
            InstalledMission data = await MissionPackage.ReadAsync(_directory, cancellationToken: token);
            token.ThrowIfCancellationRequested();
            if (!_closing) ShowGame().LoadMission(data.Definition, data.Runner, data.Hint);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { _status.Text = "Načítanie zrušené"; }
        catch (Exception error) { _status.Text = $"Misiu nemožno otvoriť. {error.Message}"; }
        finally { _cancel?.Dispose(); _cancel = null; SetBusy(false); }
    }
}
