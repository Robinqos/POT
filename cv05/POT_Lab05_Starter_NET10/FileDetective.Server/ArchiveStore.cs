using System.Runtime.CompilerServices;
using System.Text.Json;
using FileDetective.Core;

namespace FileDetective.Server;

// Hotová infraštruktúra. Archív prijíma iba opis nálezu, nikdy cestu na čítanie serverového disku.
public sealed class ArchiveStore(ILogger<ArchiveStore> log)
{
    private readonly string _folder = Environment.GetEnvironmentVariable("FILEDETECTIVE_ARCHIVE")
        ?? Path.Combine(AppContext.BaseDirectory, "Archive");

    public static bool IsValid(ReportUpload? report) => report is { CaseName.Length: > 0 and <= 100, ScannedFiles: >= 0, DuplicateBytes: >= 0, Groups.Length: <= 1000 }
        && report.Groups.All(g => g != null && g.Size >= 0 && g.Files is { Length: >= 2 and <= 10000 }
            && g.Files.All(f => !string.IsNullOrWhiteSpace(f) && f.Length <= 512 && !Path.IsPathRooted(f)));

    public async Task<ArchivedReport> SaveAsync(ReportUpload report, int delayMs, CancellationToken token)
    {
        var record = new ArchivedReport(Guid.NewGuid(), DateTimeOffset.UtcNow, report);
        Directory.CreateDirectory(_folder);
        string target = Path.Combine(_folder, record.Id + ".json");
        string staging = target + ".partial";
        log.LogInformation("ARCHIVE_STARTED {Id}", record.Id);
        try
        {
            // Riadená simulácia pomalého servera, zapnutá výhradne parametrom pokusu.
            if (delayMs > 0) await Task.Delay(delayMs, token);
            await File.WriteAllTextAsync(staging, JsonSerializer.Serialize(record), token);
            token.ThrowIfCancellationRequested();
            File.Move(staging, target);
            log.LogInformation("ARCHIVE_SAVED {Id}", record.Id);
            return record;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            log.LogInformation("ARCHIVE_CANCELLED {Id}", record.Id);
            throw;
        }
        finally { if (File.Exists(staging)) File.Delete(staging); }
    }

    public async Task<ArchivedReport?> ReadAsync(Guid id, CancellationToken token)
    {
        string path = Path.Combine(_folder, id + ".json");
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize<ArchivedReport>(await File.ReadAllTextAsync(path, token));
    }

    public async IAsyncEnumerable<ArchivedReport> ListAsync([EnumeratorCancellation] CancellationToken token)
    {
        if (!Directory.Exists(_folder)) yield break;
        foreach (var path in Directory.EnumerateFiles(_folder, "*.json").Order(StringComparer.Ordinal))
        {
            token.ThrowIfCancellationRequested();
            var report = JsonSerializer.Deserialize<ArchivedReport>(await File.ReadAllTextAsync(path, token));
            if (report != null) yield return report;
        }
    }
}
