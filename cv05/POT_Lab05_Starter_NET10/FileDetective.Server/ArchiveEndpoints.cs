using FileDetective.Core;

namespace FileDetective.Server;

public static class ArchiveEndpoints
{
    public static Task<IResult> Publish(ReportUpload report, int? delayMs, ArchiveStore store, CancellationToken token)
    {
        if (!ArchiveStore.IsValid(report) || delayMs is < 0 or > 5000)
            return Task.FromResult<IResult>(Results.BadRequest("Neplatný report alebo oneskorenie."));
        return SaveValidAsync(report, delayMs ?? 0, store, token);
    }

    // BEGIN U5SERVER
    private static Task<IResult> SaveValidAsync(ReportUpload report, int delayMs, ArchiveStore store, CancellationToken token)
    {
        // TODO U5b: pridajte async, await store.SaveAsync(report, delayMs, token).
        // Vráťte Results.Created s cestou /api/reports/{id} a uloženým objektom.
        throw new NotImplementedException("TODO U5b: Minimal API endpoint");
    }
    // END U5SERVER
}
