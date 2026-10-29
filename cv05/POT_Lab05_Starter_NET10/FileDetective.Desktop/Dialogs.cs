using System.Text;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FileDetective.Core;

namespace FileDetective.Desktop;

public sealed class PreviewWindow : ExperimentWindow
{
    private readonly ListBox _files = new() { Height = 135 };
    private readonly TextBlock _count = new();
    private IReadOnlyList<FileItem> _items = [];

    public PreviewWindow(Session session) : base("1 Preskúmať súbory", "Vyberte súbor a skontrolujte jeho skutočný obsah.")
    {
        Panel.Children.Add(_count); Panel.Children.Add(_files);
        AddStart("Načítať náhľad", async token =>
        {
            if (_files.SelectedIndex < 0) throw new InvalidOperationException("Vyberte súbor.");
            byte[] bytes = await new FileReader().ReadAsync(_items[_files.SelectedIndex].Path, token);
            bool text = !bytes.Take(1024).Any(b => b < 32 && b is not (9 or 10 or 13));
            Result.Text = $"{bytes.Length:N0} B" + Environment.NewLine + (text
                ? Encoding.UTF8.GetString(bytes, 0, Math.Min(1024, bytes.Length))
                : Convert.ToHexString(bytes.AsSpan(0, Math.Min(64, bytes.Length))));
        });
        FinishLayout();
        Opened += async (_, _) => await RunUiAsync(async token =>
        {
            _items = await FileCatalog.ListAsync(session.Root, token);
            _files.ItemsSource = _items.Select(f => Path.GetRelativePath(session.Root, f.Path)).ToArray();
            if (_items.Count > 0) _files.SelectedIndex = 0;
            _count.Text = $"{_items.Count} súborov · {_items.Sum(f => f.Length):N0} B";
        });
    }
}

public abstract class ScanWindow : ExperimentWindow
{
    private readonly TextBox _budget = new() { Text = "5000", Width = 100 };
    private readonly TextBlock _progress = new() { Text = "Čakám na spustenie" };

    protected ScanWindow(Session session, bool limited) : base(limited ? "3 Audit s časovým limitom" : "2 Nájsť kópie",
        "Porovnanie obsahu podľa veľkosti a SHA256. Vstupné súbory sa nemenia.")
    {
        if (limited) Panel.Children.Add(Row(new TextBlock { Text = "Časový limit v ms" }, _budget));
        Panel.Children.Add(_progress);
        AddStart(limited ? "Spustiť audit" : "Hľadať kópie", async token =>
        {
            int budget = 5000;
            if (limited && (!int.TryParse(_budget.Text, out budget) || budget < 1 || budget > 600000))
                throw new ArgumentException("Limit musí byť od 1 do 600000 ms.");
            var scanner = session.CreateScanner();
            var progress = new Progress<ScanProgress>(p => _progress.Text = $"{p.Done}/{p.Total} · {p.FileName}");
            ScanReport report;
            if (limited)
            {
                using var deadline = new CancellationTokenSource(); deadline.CancelAfter(budget);
                report = await new BudgetScanner(scanner).RunAsync(session.Root, token, deadline.Token, progress);
            }
            else report = await scanner.ScanAsync(session.Root, progress, token);
            session.Report = report;
            Result.Text = SummaryText.Build(report);
        });
        Result.MinHeight = 210;
        FinishLayout();
    }
}

public sealed class DuplicateWindow(Session session) : ScanWindow(session, false);
public sealed class BudgetWindow(Session session) : ScanWindow(session, true);

public sealed class ExportWindow : ExperimentWindow
{
    private readonly Button _yes = new() { Content = "Uložiť", IsEnabled = false };
    private readonly Button _no = new() { Content = "Neukladať", IsEnabled = false };
    private readonly Button _open = new() { Content = "Otvoriť report", IsEnabled = false };
    private ExportDecision? _decision;

    public ExportWindow(Session session) : base("4 Uložiť report", "Prezrite nález, potom rozhodnite o vytvorení HTML reportu.")
    {
        Result.Text = session.Report is { } report ? SummaryText.Build(report) : "Najprv dokončite hľadanie kópií.";
        AddStart("Pripraviť uloženie", async token =>
        {
            if (session.Report is not { } current) throw new InvalidOperationException("Najprv dokončite hľadanie.");
            _decision = new ExportDecision(); _yes.IsEnabled = _no.IsEnabled = true;
            Log("Uložiť tento nález do nového HTML reportu?");
            if (await _decision.WaitAsync(token))
            {
                session.ExportPath = await ReportExporter.SaveAsync(current, session.OutputFolder, token);
                Log("Report uložený: " + session.ExportPath); _open.IsEnabled = true;
            }
            else Log("Report nebol uložený.");
        }, _yes, _no);
        Panel.Children.Add(_open);
        _yes.Click += (_, _) => Decide(true);
        _no.Click += (_, _) => Decide(false);
        _open.Click += async (_, _) =>
        {
            try { if (session.ExportPath != null) await Launcher.LaunchFileInfoAsync(new FileInfo(session.ExportPath)); }
            catch (Exception ex) { Log(ex.Message); }
        };
        Result.MinHeight = 180;
        FinishLayout();
    }

    private void Decide(bool save)
    {
        try { _decision?.TryDecide(save); }
        catch (Exception ex) { _decision?.TryFail(ex); }
    }

    protected override void ResetOperation() { _decision = null; _yes.IsEnabled = _no.IsEnabled = false; }
}
