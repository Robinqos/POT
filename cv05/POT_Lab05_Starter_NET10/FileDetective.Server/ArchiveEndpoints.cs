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

    private static async Task<IResult> SaveValidAsync(ReportUpload report, int delayMs, ArchiveStore store, CancellationToken token)
    {
        ArchivedReport saved = await store
            .SaveAsync(report, delayMs, token)
            .ConfigureAwait(false);

        return Results.Created($"/api/reports/{saved.Id}", saved);
    }
}
