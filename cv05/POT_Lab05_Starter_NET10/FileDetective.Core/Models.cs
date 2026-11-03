namespace FileDetective.Core;

public sealed record FileItem(string Path, long Length);
public sealed record FingerprintEntry(string Path, long Length, string Hash);
public sealed record ScanProgress(int Done, int Total, string FileName);
public sealed record ScanIssue(string Path, string Message);
public sealed record DuplicateGroup(long Length, string Hash, IReadOnlyList<string> Paths);

public sealed record ScanReport(string Root, int ScannedFiles, long ReadBytes,
    IReadOnlyList<DuplicateGroup> Groups, IReadOnlyList<ScanIssue> Issues)
{
    public int ExtraCopies => Groups.Sum(g => g.Paths.Count - 1);
    public long DuplicateBytes => Groups.Sum(g => g.Length * (g.Paths.Count - 1));

    // Pripravené zoskupenie. Názov súboru o rovnakom obsahu nerozhoduje.
    public static ScanReport Create(string root, IReadOnlyList<FingerprintEntry> entries, IReadOnlyList<ScanIssue> issues)
    {
        var groups = entries.GroupBy(e => (e.Length, e.Hash)).Where(g => g.Count() > 1)
            .Select(g => new DuplicateGroup(g.Key.Length, g.Key.Hash,
                g.Select(e => e.Path).Order(StringComparer.Ordinal).ToArray()))
            .OrderByDescending(g => g.Length * (g.Paths.Count - 1)).ThenBy(g => g.Hash, StringComparer.Ordinal).ToArray();
        return new ScanReport(root, entries.Count, entries.Sum(e => e.Length), groups, issues.ToArray());
    }
}
