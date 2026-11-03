using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Platform.Storage;

namespace FileDetective.Desktop;

public sealed class MainWindow : Window
{
    private readonly Session _session;
    private readonly TextBlock _folder = new() { TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly TextBlock _result = new() { TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly Button _export;
    private readonly List<Button> _buttons = [];

    public MainWindow() : this(new Session(Session.DemoFolder())) { }

    public MainWindow(Session session)
    {
        _session = session;
        Title = "Lab05 | Audit súborov";
        Width = 630; Height = 670; MinWidth = 600; MinHeight = 600;
        var panel = new StackPanel { Margin = new Thickness(26), Spacing = 15 };
        panel.Children.Add(new TextBlock { Text = "Audit súborov", FontSize = 28 });
        panel.Children.Add(new TextBlock { Text = "Odhaľte skryté kópie a zdieľajte nález." });
        var pick = Action("Vlastný priečinok", async () =>
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });
            string? path = folders.FirstOrDefault()?.TryGetLocalPath();
            if (path != null) _session.SelectFolder(path);
        });
        var demo = Action("Ukážkové súbory", () => { _session.SelectFolder(Session.DemoFolder()); return Task.CompletedTask; });
        panel.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { pick, demo } });
        panel.Children.Add(_folder);
        panel.Children.Add(Open("1  Preskúmať súbory", () => new PreviewWindow(_session)));
        panel.Children.Add(Open("2  Nájsť kópie", () => new DuplicateWindow(_session)));
        panel.Children.Add(Open("3  Audit s časovým limitom", () => new BudgetWindow(_session)));
        _export = Open("4  Uložiť report", () => new ExportWindow(_session));
        panel.Children.Add(_export);
        panel.Children.Add(Open("5  Tímový archív", () => new ArchiveWindow(_session)));
        panel.Children.Add(_result);
        panel.Children.Add(Action("Pripraviť väčší prípad 128 MiB", async () =>
        {
            _result.Text = "Pripravujem väčšie dáta...";
            _session.SelectFolder(await Session.PrepareLargeAsync(default));
        }));
        Content = new ScrollViewer { Content = panel };
        Refresh();
    }

    private Button Open(string title, Func<Window> create) => Action(title, async () => await create().ShowDialog(this));

    private Button Action(string title, Func<Task> action)
    {
        var button = new Button { Content = title, HorizontalAlignment = HorizontalAlignment.Stretch };
        _buttons.Add(button);
        button.Click += async (_, _) =>
        {
            foreach (var item in _buttons) item.IsEnabled = false;
            try { await action(); Refresh(); }
            catch (Exception ex) { _result.Text = ex.Message; }
            finally
            {
                foreach (var item in _buttons) item.IsEnabled = true;
                _export.IsEnabled = _session.Report != null;
            }
        };
        return button;
    }

    private void Refresh()
    {
        _folder.Text = "Priečinok: " + _session.Root;
        _export.IsEnabled = _session.Report != null;
        _result.Text = _session.Report is { } report
            ? $"Skupiny kópií: {report.Groups.Count} · nadbytočné kópie: {report.ExtraCopies}. Report je pripravený."
            : "Začnite náhľadom. Report sa sprístupní po dokončenom hľadaní.";
    }
}
