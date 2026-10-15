using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace NeonovyKurier.Desktop;

// Pripravené okno na získanie misie. Študent upravuje službu, nie UI.
public sealed partial class MainWindow
{
    private readonly Button _install = new() { Name = "Install", Content = "Stiahnuť misiu" };
    private readonly Button _cancelButton = new() { Name = "Cancel", Content = "Zrušiť", IsEnabled = false };
    private readonly Button _play = new() { Name = "Play", Content = "Hrať misiu", IsEnabled = false };
    private readonly Button _training = new() { Name = "Training", Content = "Tréning ovládania" };
    private readonly Button _one = new() { Name = "One", Content = "Stiahnuť iba mapu" };
    private readonly Button _download = new() { Name = "Download", Content = "Stiahnuť bez uloženia" };
    private readonly ComboBox _mode = new()
    {
        Name = "Mode", SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Stretch,
        ItemsSource = new[] { "Postupne", "Súbežne", "Súbežne s cache", "Najviac 2 naraz" }
    };
    private readonly CheckBox _failure = new() { Name = "Failure", Content = "Výpadok servera so vzhľadom" };
    private readonly CheckBox _slow = new() { Name = "Slow", Content = "Pomalá sieť - 5 s na súbor" };
    private readonly TextBlock _status = new() { Name = "Status", Text = "Spúšťam lokálne služby...", FontSize = 17, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _missionState = new() { Name = "MissionState", FontSize = 13, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _pulse = Text("UI žije: 0", 13);
    private readonly TextBlock _clicks = Text("Reakcie: 0", 13);
    private readonly TextBlock _summary = Text("0 z 3 súborov", 13);
    private readonly ProgressBar _progressBar = new() { Minimum = 0, Maximum = 3, Height = 8 };
    private readonly Dictionary<string, TextBlock> _states = new()
    {
        ["level"] = Text("čaká", 14), ["theme"] = Text("čaká", 14), ["tips"] = Text("čaká", 14)
    };
    private readonly TextBox _log = new()
    {
        Name = "Log", IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap,
        MinHeight = 140, MaxHeight = 220, FontSize = 12,
        FontFamily = new FontFamily("Consolas, Menlo, monospace")
    };
    private static TextBlock Text(string value, double size) => new()
        { Text = value, FontSize = size, TextWrapping = TextWrapping.Wrap };
    private static StackPanel Stack(params Control[] children)
    {
        var panel = new StackPanel { Spacing = 12 };
        foreach (var child in children) panel.Children.Add(child);
        return panel;
    }
    private static WrapPanel Row(params Control[] children)
    {
        var row = new WrapPanel();
        foreach (var child in children) { child.Margin = new Thickness(0, 0, 10, 6); row.Children.Add(child); }
        return row;
    }
    private void BuildLayout()
    {
        Title = "Neónový kuriér - príprava misie";
        Width = 680; Height = 680; MinWidth = 560; MinHeight = 600;
        Background = Brush.Parse("#0B1220"); Foreground = Brushes.White;
        var files = Stack();
        foreach (var (id, name) in new[] { ("level", "Mapa misie (level.json) - povinná"), ("theme", "Vzhľad kuriéra (theme.json)"), ("tips", "Pokyny (tips.txt)") })
        {
            var line = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            line.Children.Add(Text(name, 14)); Grid.SetColumn(_states[id], 1);
            line.Children.Add(_states[id]); files.Children.Add(line);
        }
        _install.Click += InstallClicked; _one.Click += OneClicked; _download.Click += DownloadClicked;
        _cancelButton.Click += (_, _) => _cancel?.Cancel();
        _play.Click += PlayClicked;
        _training.Click += (_, _) => ShowTraining();
        var alive = new Button { Name = "Responsive", Content = "Skúsiť odozvu +1" };
        alive.Click += (_, _) => _clicks.Text = $"Reakcie: {++_responseCount}";
        var sleep = new Button { Name = "BlockingProbe", Content = "Sleep 1,5 s" };
        sleep.Click += (_, _) => { if (!_busy && !_closing) Thread.Sleep(1500); };
        var delay = new Button { Name = "AwaitProbe", Content = "Await 1,5 s" };
        delay.Click += AwaitProbeClicked;
        var experiments = new Expander
        {
            Name = "Experiments", Header = "Pokusy a diagnostika", IsExpanded = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Content = Stack(
                Text("U2 / U3 - HTTP bez uloženia", 13), Row(_one, _download),
                Text("U5 - sieťové podmienky", 13), _failure, _slow,
                Text("U1 - odozva oboch okien", 13), Row(_pulse, _clicks), Row(alive, sleep, delay),
                Text("Pri Sleep sa obe okná zastavia. Otvor tréning pred začatím pokusu.", 12),
                Text("Časová stopa START / DONE / CACHE", 13), _log)
        };
        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(24), Spacing = 16,
                Children =
                {
                    Text("NEÓNOVÝ KURIÉR", 28),
                    Text("Doruč všetky zásielky. Šetri energiu. Najprv získaj údaje hlavnej misie.", 14),
                    _status, _mode, files, _progressBar, _summary,
                    Row(_install, _cancelButton, _play), _missionState,
                    Row(_training), experiments
                }
            }
        };
    }
}
