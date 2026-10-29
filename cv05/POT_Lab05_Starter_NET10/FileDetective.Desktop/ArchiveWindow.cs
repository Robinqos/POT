using Avalonia.Controls;
using System.Net.Http.Json;
using FileDetective.Core;

namespace FileDetective.Desktop;

public sealed class ArchiveWindow : ExperimentWindow
{
    private readonly HttpClient _http;
    private readonly StatusRecorder? _status;
    private readonly CheckBox _slow = new() { Content = "Pomalý archív 3 s" };
    private readonly TextBox _id = new() { PlaceholderText = "ID publikácie", MinWidth = 390 };

    public ArchiveWindow(Session session, HttpClient? http = null) : base("5 Tímový archív", "Zdieľajte opis nálezu a načítajte ho späť zo služby.")
    {
        Height = 760;
        _http = http ?? new HttpClient(_status = new StatusRecorder())
            { BaseAddress = new Uri("http://127.0.0.1:5085/"), Timeout = Timeout.InfiniteTimeSpan };
        Panel.Children.Add(_slow);
        var client = new ArchiveClient(_http);
        AddStart("Publikovať nález", async token =>
        {
            if (session.Report is not { } report) throw new InvalidOperationException("Najprv dokončite hľadanie kópií.");
            var saved = await client.PublishAsync(report, _slow.IsChecked == true, token);
            _id.Text = saved.Id.ToString();
            Log($"{_status?.Last ?? "Publikované"} · ID {saved.Id}{_status?.Location}");
        });
        Panel.Children.Add(_id);
        AddStart("Načítať publikáciu", async token =>
        {
            if (!Guid.TryParse(_id.Text, out Guid id)) throw new ArgumentException("Zadajte platné ID publikácie.");
            var saved = await client.ReadAsync(id, token);
            Log($"{_status?.Last ?? "Načítané"} · {saved.Report.CaseName} · skupiny: {saved.Report.Groups.Length}");
        });
        AddStart("Stav a zoznam", async token =>
        {
            var reports = await client.CheckAsync(token);
            Result.Text = "Archív je dostupný. Publikácie: " + reports.Count + Environment.NewLine
                + string.Join(Environment.NewLine, reports.Select(r => $"{r.Id} · {r.Report.CaseName}"));
        });
        var experiment = new Button { Content = "Zablokovať okno na 3 s" };
        experiment.Click += (_, _) =>
        {
            if (IsBusy) return;
            try
            {
                if (session.Report == null) throw new InvalidOperationException("Najprv dokončite hľadanie.");
                using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                // Zámerný ohraničený protipríklad nad BCL Taskom, nie nad študentovou async metódou.
                using var response = _http.PostAsJsonAsync("api/reports?delayMs=3000",
                    ReportUpload.From(session.Report), limit.Token).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                Log("Blokujúci pokus skončil. Aj on vytvoril publikáciu. Porovnajte odozvu s await.");
            }
            catch (Exception ex) { Log(ex.Message); }
        };
        Panel.Children.Add(new Expander { Header = "Experiment odozvy", Content = experiment });
        FinishLayout();
        Closed += (_, _) => _http.Dispose();
    }
}

// Okno ukazuje skutočný stav poslednej odpovede, nie očakávaný text.
internal sealed class StatusRecorder() : DelegatingHandler(new SocketsHttpHandler())
{
    public string? Last { get; private set; }
    public string? Location { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        var response = await base.SendAsync(request, token).ConfigureAwait(false);
        Last = $"{(int)response.StatusCode} {response.ReasonPhrase}";
        Location = response.Headers.Location is { } location ? $" · Location {location}" : null;
        return response;
    }
}
