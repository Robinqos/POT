namespace FileDetective.Core;

public sealed class BudgetScanner(IScanner scanner)
{
    // BEGIN U3
    public async Task<ScanReport> RunAsync(string root, CancellationToken userToken,
    CancellationToken deadlineToken, IProgress<ScanProgress>? progress = null)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(userToken, deadlineToken);
        try
        {
            // Overenie pred spustením – ak už niečo je zrušené, hneď reaguj
            linked.Token.ThrowIfCancellationRequested();

            return await scanner.ScanAsync(root, progress, linked.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Používateľ má prednosť – aj keď sú zrušené oba
            if (userToken.IsCancellationRequested)
                throw new OperationCanceledException(userToken);

            // Deadline – preklad na TimeoutException
            if (deadlineToken.IsCancellationRequested)
                throw new TimeoutException("Audit prekročil časový limit.");

            // OCE z iného dôvodu (napr. interné zrušenie v scanneri) – zachovaj
            throw;
        }
    }
    // END U3
}
