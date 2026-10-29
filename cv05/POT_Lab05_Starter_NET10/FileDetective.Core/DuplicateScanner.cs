namespace FileDetective.Core;

public interface IScanner
{
    Task<ScanReport> ScanAsync(string root, IProgress<ScanProgress>? progress, CancellationToken token);
}

public sealed class DuplicateScanner(IFileReader reader) : IScanner
{
    // BEGIN U2B
    public Task<FingerprintEntry> HashFileAsync(string path, CancellationToken token)
    {
        _ = reader;
        // TODO U2b: await reader.ReadAsync, potom await Task.Run pre Fingerprint.Compute.
        // Vráťte FingerprintEntry(path, bytes.LongLength, hash). Token patrí obom častiam.
        throw new NotImplementedException("TODO U2b: nájdite kópie podľa obsahu");
    }
    // END U2B

    // Pripravená infraštruktúra: prehľad, slučka, chyby súborov a zoskupenie.
    public async Task<ScanReport> ScanAsync(string root, IProgress<ScanProgress>? progress, CancellationToken token)
    {
        var files = await FileCatalog.ListAsync(root, token).ConfigureAwait(false);
        var entries = new List<FingerprintEntry>();
        var issues = new List<ScanIssue>();
        int done = 0;
        foreach (var file in files)
        {
            token.ThrowIfCancellationRequested();
            try { entries.Add(await HashFileAsync(file.Path, token).ConfigureAwait(false)); }
            catch (IOException ex) { issues.Add(new ScanIssue(file.Path, ex.Message)); }
            catch (UnauthorizedAccessException ex) { issues.Add(new ScanIssue(file.Path, ex.Message)); }
            progress?.Report(new ScanProgress(++done, files.Count, System.IO.Path.GetFileName(file.Path)));
        }
        token.ThrowIfCancellationRequested();
        return ScanReport.Create(root, entries, issues);
    }
}

public static class FileCatalog
{
    public const int MaxFiles = 10000;

    // Directory nemá async enumeráciu. Synchrónne prechádzanie presúvame mimo UI.
    public static Task<IReadOnlyList<FileItem>> ListAsync(string root, CancellationToken token) => Task.Run<IReadOnlyList<FileItem>>(() =>
    {
        var directory = new DirectoryInfo(root);
        if (!directory.Exists) throw new DirectoryNotFoundException(root);
        var result = new List<FileItem>();
        var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint };
        foreach (var file in directory.EnumerateFiles("*", options))
        {
            token.ThrowIfCancellationRequested();
            if (result.Count >= MaxFiles) throw new IOException("Priečinok prekročil limit 10000 súborov.");
            result.Add(new FileItem(file.FullName, file.Length));
        }
        token.ThrowIfCancellationRequested();
        return result.OrderBy(f => f.Path, StringComparer.Ordinal).ToArray();
    }, token);
}
