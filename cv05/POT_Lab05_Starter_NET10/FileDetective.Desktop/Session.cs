using FileDetective.Core;

namespace FileDetective.Desktop;

public sealed class Session(string root)
{
    public string Root { get; private set; } = root;
    public ScanReport? Report { get; set; }
    public string? ExportPath { get; set; }
    public string OutputFolder { get; init; } = Path.Combine(AppContext.BaseDirectory, "Reports");
    public Func<IScanner> CreateScanner { get; init; } = () => new DuplicateScanner(new FileReader());
    public void SelectFolder(string root) { Root = root; Report = null; ExportPath = null; }

    public static string DemoFolder()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "FileDetective.slnx")))
                return Path.Combine(dir.FullName, "DemoData");
        return Path.Combine(AppContext.BaseDirectory, "DemoData");
    }

    // Väčšie dáta sú voliteľné. ZIP zostáva malý; vytvárajú sa iba na kliknutie.
    public static async Task<string> PrepareLargeAsync(CancellationToken token)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Workspace", "VelkyPripad");
        Directory.CreateDirectory(root);
        for (int i = 0; i < 32; i++)
        {
            token.ThrowIfCancellationRequested();
            int seed = i / 4;
            byte[] bytes = await Task.Run(() =>
            {
                var data = new byte[4 * 1024 * 1024];
                new Random(700 + seed).NextBytes(data);
                return data;
            }, token);
            await File.WriteAllBytesAsync(Path.Combine(root, $"zaznam-{i:00}.bin"), bytes, token);
        }
        return root;
    }
}

public static class SummaryText
{
    public static string Build(ScanReport report)
    {
        var lines = new List<string>
        {
            $"Prečítané súbory: {report.ScannedFiles} · skupiny kópií: {report.Groups.Count}",
            $"Nadbytočné kópie: {report.ExtraCopies} · potenciálna úspora {report.DuplicateBytes:N0} B",
            ""
        };
        int index = 0;
        foreach (var group in report.Groups)
        {
            lines.Add($"Skupina {++index} · počet súborov: {group.Paths.Count} · veľkosť: {group.Length:N0} B");
            lines.AddRange(group.Paths.Select(p => "  " + Path.GetRelativePath(report.Root, p)));
            lines.Add("");
        }
        foreach (var issue in report.Issues) lines.Add($"Preskočené: {Path.GetRelativePath(report.Root, issue.Path)} · {issue.Message}");
        return string.Join(Environment.NewLine, lines);
    }
}
