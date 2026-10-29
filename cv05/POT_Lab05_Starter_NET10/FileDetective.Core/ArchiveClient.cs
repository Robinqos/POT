using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace FileDetective.Core;

public sealed record ArchiveGroup(long Size, string[] Files);
public sealed record ReportUpload(string CaseName, int ScannedFiles, long DuplicateBytes, ArchiveGroup[] Groups)
{
    public static ReportUpload From(ScanReport report) => new(
        Path.GetFileName(Path.TrimEndingDirectorySeparator(report.Root)), report.ScannedFiles, report.DuplicateBytes,
        report.Groups.Select(g => new ArchiveGroup(g.Length, g.Paths.Select(p => Path.GetRelativePath(report.Root, p)).ToArray())).ToArray());
}
public sealed record ArchivedReport(Guid Id, DateTimeOffset CreatedAt, ReportUpload Report);

public sealed class ArchiveClient(HttpClient http)
{
    // BEGIN U5CLIENT
    public async Task<ArchivedReport> PublishAsync(ScanReport report, bool slow, CancellationToken token)
    {
        _ = http;
        // TODO U5a: await PostAsJsonAsync s ReportUpload.From(report) a tokenom.
        // Overte HTTP stav a await ReadFromJsonAsync<ArchivedReport> s tokenom.
        string url = slow ? "api/reports?delayMs=3000" : "api/reports";

        using var response = await http
            .PostAsJsonAsync(url, ReportUpload.From(report), token)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ArchivedReport>(token)
            .ConfigureAwait(false)
            ?? throw new InvalidDataException("Archív vrátil prázdnu odpoveď.");
    }
    // END U5CLIENT

    public async Task<ArchivedReport> ReadAsync(Guid id, CancellationToken token) =>
        await http.GetFromJsonAsync<ArchivedReport>($"api/reports/{id}", token).ConfigureAwait(false)
        ?? throw new InvalidDataException("Archív vrátil prázdnu odpoveď.");

    public async IAsyncEnumerable<ArchivedReport> ListAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (var item in http.GetFromJsonAsAsyncEnumerable<ArchivedReport>("api/reports", token).ConfigureAwait(false))
            if (item != null) yield return item;
    }

    private async Task HealthAsync(CancellationToken token)
    {
        using var response = await http.GetAsync("health", token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private async Task<List<ArchivedReport>> CollectAsync(CancellationToken token)
    {
        var reports = new List<ArchivedReport>();
        await foreach (var report in ListAsync(token).ConfigureAwait(false)) reports.Add(report);
        return reports;
    }

    // Hotový pokus: nezávislé požiadavky začneme pred spoločným await.
    public async Task<IReadOnlyList<ArchivedReport>> CheckAsync(CancellationToken token)
    {
        Task health = HealthAsync(token);
        Task<List<ArchivedReport>> list = CollectAsync(token);
        await Task.WhenAll(health, list).ConfigureAwait(false);
        return await list.ConfigureAwait(false);
    }
}
