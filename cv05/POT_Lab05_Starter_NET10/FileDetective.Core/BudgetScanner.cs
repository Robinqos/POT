namespace FileDetective.Core;

public sealed class BudgetScanner(IScanner scanner)
{
    // BEGIN U3
    public Task<ScanReport> RunAsync(string root, CancellationToken userToken,
        CancellationToken deadlineToken, IProgress<ScanProgress>? progress = null)
    {
        _ = scanner;
        // TODO U3: prepojené tokeny, await scanner.ScanAsync; používateľ má prednosť.
        // Používateľ => OCE s userToken; deadline => TimeoutException. Ostatné chyby zachovajte.
        throw new NotImplementedException("TODO U3: rušenie a časový rozpočet auditu");
    }
    // END U3
}
