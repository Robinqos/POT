using System.Net;
using System.Text;

namespace FileDetective.Core;

public static class ReportExporter
{
    // Hotové formátovanie HTML, skutočný asynchrónny zápis a žiadne mazanie vstupov.
    public static string ToHtml(ScanReport report)
    {
        static string E(string text) => WebUtility.HtmlEncode(text);
        var html = new StringBuilder("<!doctype html><html lang=\"sk\"><meta charset=\"utf-8\"><title>Audit súborov</title><style>body{font:16px system-ui;max-width:960px;margin:40px auto;padding:0 24px;color:#163047}h1{font-size:32px}section{margin:24px 0;padding:16px;background:#f0f5f8;border-radius:8px}li{margin:8px 0;overflow-wrap:anywhere}</style><h1>Audit súborov</h1>");
        html.Append($"<p>Priečinok: {E(report.Root)}</p><p>Prečítané súbory: {report.ScannedFiles}. Skupiny kópií: {report.Groups.Count}. Nadbytočné kópie: {report.ExtraCopies}. Potenciálna úspora: {report.DuplicateBytes:N0} B.</p>");
        html.Append("<p>Zhoda je určená veľkosťou a odtlačkom SHA256. Súbory neboli zmenené ani vymazané.</p>");
        foreach (var group in report.Groups)
        {
            html.Append($"<section><h2>Počet súborov: {group.Paths.Count} · veľkosť: {group.Length:N0} B</h2><ul>");
            foreach (var path in group.Paths) html.Append($"<li>{E(System.IO.Path.GetRelativePath(report.Root, path))}</li>");
            html.Append("</ul></section>");
        }
        if (report.Issues.Count > 0)
        {
            html.Append("<h2>Preskočené súbory</h2><ul>");
            foreach (var issue in report.Issues) html.Append($"<li>{E(issue.Path)}: {E(issue.Message)}</li>");
            html.Append("</ul>");
        }
        return html.Append("</html>").ToString();
    }

    public static async Task<string> SaveAsync(ScanReport report, string outputFolder, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        Directory.CreateDirectory(outputFolder);
        string target = Path.Combine(outputFolder, $"report-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.html");
        bool created = false;
        try
        {
            await using var file = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, FileOptions.Asynchronous);
            created = true;
            await using var writer = new StreamWriter(file, new UTF8Encoding(false));
            await writer.WriteAsync(ToHtml(report).AsMemory(), token).ConfigureAwait(false);
        }
        catch
        {
            // Upratujeme výhradne nový vlastný neúplný report, nikdy vstupné súbory.
            if (created && File.Exists(target)) File.Delete(target);
            throw;
        }
        return target;
    }
}
