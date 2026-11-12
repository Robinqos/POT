using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using DevPulse.Core.Models;

namespace DevPulse.Core.Services;

/// <summary>
/// Produkčná implementácia IFeedService využívajúca HttpClient a LINQ to XML (XDocument).
/// Nahrádza zastaraný XmlDataProvider z pôvodného WPF cvičenia plne asynchrónnym a multiplatformovým riešením.
/// </summary>
public sealed partial class HttpFeedService : IFeedService
{
    public const string OfflineDemoUrl = "offline://fri-pot";

    private readonly HttpClient _httpClient;

    public HttpFeedService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? CreateDefaultClient();
    }

    private static HttpClient CreateDefaultClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("DevPulse-FRI-UNIZA/1.0");
        return client;
    }

    public IReadOnlyList<FeedSource> GetDefaultSources() =>
    [
        new(".NET Blog (Microsoft RSS)", "https://devblogs.microsoft.com/dotnet/feed/"),
        new("Avalonia UI Releases (Atom)", "https://github.com/AvaloniaUI/Avalonia/releases.atom"),
        new("Živé.sk – Najnovšie (RSS)", "https://zive.aktuality.sk/rss/najnovsie/"),
        new("Offline Demo – FRI UNIZA & POT", OfflineDemoUrl)
    ];

    public async Task<IReadOnlyList<FeedArticle>> LoadFeedAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.Equals(url, OfflineDemoUrl, StringComparison.OrdinalIgnoreCase))
        {
            await Task.Delay(250, cancellationToken);
            return GetOfflineArticles();
        }

        Uri.TryCreate(url, UriKind.Absolute, out var feedBaseUri);
        await using var stream = await _httpClient.GetStreamAsync(url, cancellationToken);

        XDocument doc;
        try
        {
            doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException("Odpoveď servera nie je platný XML dokument (možný prihlasovací portál siete).", ex);
        }

        var rootName = doc.Root?.Name.LocalName;
        if (!string.Equals(rootName, "rss", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(rootName, "RDF", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(rootName, "feed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Odpoveď servera nie je RSS ani Atom kanál (koreňový element <{rootName ?? "?"}>).");
        }

        // Podpora pre RSS 2.0 / RDF (<item>) aj Atom (<entry>)
        var rssItems = doc.Descendants().Where(e => string.Equals(e.Name.LocalName, "item", StringComparison.OrdinalIgnoreCase)).ToList();
        if (rssItems.Count > 0)
        {
            return rssItems
                .Take(15)
                .Select(item => new FeedArticle
                {
                    Title = CleanText(item.Elements().FirstOrDefault(e => e.Name.LocalName == "title")?.Value) ?? "(Bez názvu)",
                    Link = ResolveLink(item.Elements().FirstOrDefault(e => e.Name.LocalName == "link")?.Value, feedBaseUri),
                    Summary = Truncate(CleanText(item.Elements().FirstOrDefault(e => e.Name.LocalName == "description")?.Value) ?? "", 220),
                    PublishedText = FormatDate(item.Elements().FirstOrDefault(e => e.Name.LocalName is "pubDate" or "date")?.Value)
                })
                .ToList();
        }

        XNamespace atom = "http://www.w3.org/2005/Atom";
        return doc.Descendants(atom + "entry")
            .Take(15)
            .Select(entry =>
            {
                var links = entry.Elements(atom + "link").ToList();
                var href = links
                    .Where(l => string.IsNullOrEmpty(l.Attribute("rel")?.Value) ||
                                string.Equals(l.Attribute("rel")?.Value, "alternate", StringComparison.OrdinalIgnoreCase))
                    .Select(l => l.Attribute("href")?.Value)
                    .FirstOrDefault(h => !string.IsNullOrWhiteSpace(h))
                    ?? links.Select(l => l.Attribute("href")?.Value)
                        .FirstOrDefault(h => !string.IsNullOrWhiteSpace(h));

                var summary = entry.Element(atom + "summary")?.Value
                              ?? entry.Element(atom + "content")?.Value
                              ?? "";

                return new FeedArticle
                {
                    Title = CleanText(entry.Element(atom + "title")?.Value) ?? "(Bez názvu)",
                    Link = ResolveLink(href, feedBaseUri),
                    Summary = Truncate(CleanText(summary) ?? "", 220),
                    PublishedText = FormatDate(entry.Element(atom + "updated")?.Value ?? entry.Element(atom + "published")?.Value)
                };
            })
            .ToList();
    }

    public async Task<ArticleMetrics> AnalyzeArticleAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url) || url.StartsWith("offline://", StringComparison.OrdinalIgnoreCase))
        {
            // Simulované asynchrónne meranie pre offline ukážku (umožňuje pekne vyskúšať ProgressBar a Cancel!)
            var simulatedMs = Random.Shared.Next(220, 520);
            await Task.Delay(simulatedMs, cancellationToken).ConfigureAwait(false);
            var simulatedBytes = Random.Shared.Next(18_000, 95_000);
            var minutes = Math.Max(2, simulatedBytes / 12_000);
            return new ArticleMetrics(simulatedBytes, simulatedMs, minutes);
        }

        var sw = Stopwatch.StartNew();
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        sw.Stop();

        var plainChars = Math.Max(800, bytes.Length / 4);
        var estimatedMinutes = Math.Clamp(plainChars / 1200, 1, 25);
        return new ArticleMetrics(bytes.LongLength, sw.ElapsedMilliseconds, estimatedMinutes);
    }

    public async Task AnalyzeArticlesAsync(
        IReadOnlyList<FeedArticle> articles,
        IProgress<AnalysisProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var total = articles.Count;
        for (var i = 0; i < total; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var article = articles[i];
            var completed = i + 1;

            // Meranie a hlásenie priebehu prebieha na pracovnom vlákne mimo UI vlákna (prednáška WPF, slajd 17);
            // Progress<T> bezpečne prenesie callback späť na SynchronizationContext UI vlákna.
            var metrics = await Task.Run(async () =>
            {
                var measured = await AnalyzeArticleAsync(article.Link, cancellationToken).ConfigureAwait(false);
                progress?.Report(new AnalysisProgress(completed, total, article.Title));
                return measured;
            }, cancellationToken);

            article.ApplyMetrics(metrics);
        }
    }

    public static IReadOnlyList<FeedArticle> GetOfflineArticles() =>
    [
        new()
        {
            Title = "Avalonia UI 12 + .NET 10: Jeden C# ViewModel pre Windows, macOS aj Linux",
            Link = "offline://fri-pot/avalonia-12",
            PublishedText = "Dnes • Prednáška & Cvičenie POT",
            Summary = "Compiled Bindings (x:DataType) kontrolujú preklepy už počas kompilácie a prinášajú vyšší výkon než klasická reflexia."
        },
        new()
        {
            Title = "CommunityToolkit.Mvvm a C# Source Generátory v praxi",
            Link = "offline://fri-pot/mvvm-toolkit",
            PublishedText = "Prednáška 07 • Slajd 20",
            Summary = "Atribúty [ObservableProperty] a [RelayCommand] vygenerujú INotifyPropertyChanged aj ICommand bez ručného opakovania kódu."
        },
        new()
        {
            Title = "Prečo UI vlákno nikdy nesmie zamrznúť: async/await, IProgress<T> a CancellationToken",
            Link = "offline://fri-pot/async-gui",
            PublishedText = "Prednáška 07 • Slajdy 15 – 19",
            Summary = "Dlhotrvajúce I/O a výpočty spúšťame asynchrónne, priebeh hlásime cez Progress<T> a používateľovi vždy ponúkneme možnosť zrušenia."
        },
        new()
        {
            Title = "Testovanie ViewModelu bez GUI: sila Dependency Injection a Fake služieb",
            Link = "offline://fri-pot/unit-testy",
            PublishedText = "Prednáška 07 • Slajdy 19 & 21",
            Summary = "Keď ViewModel nepozná MessageBox ani konkrétne okno, desiatky xUnit testov zbehnú v termináli za zlomok sekundy."
        },
        new()
        {
            Title = "AI Agenti pri vývoji GUI: Ako zabrániť halucináciám pomocou AGENTS.md",
            Link = "offline://fri-pot/ai-engineering",
            PublishedText = "KST FRI UNIZA • Inžinierske štúdium",
            Summary = "Bez jasných pravidiel AI často mieša starý WPF XAML s Avaloniou. Architektonický kontext mení AI na spoľahlivého kolegu."
        }
    ];

    private static string ResolveLink(string? rawLink, Uri? baseUri)
    {
        var trimmed = rawLink?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return "";

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute))
            return absolute.AbsoluteUri;

        if (baseUri is not null && Uri.TryCreate(baseUri, trimmed, out var resolved))
            return resolved.AbsoluteUri;

        return trimmed;
    }

    public static string? CleanText(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var candidate = !raw.Contains('<') && EncodedHtmlTagRegex().IsMatch(raw)
            ? WebUtility.HtmlDecode(raw)
            : raw;

        var withoutTags = HtmlTagRegex().Replace(candidate, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return WhitespaceRegex().Replace(decoded, " ").Trim();
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength).TrimEnd(), "…");

    private static string FormatDate(string? rawDate)
        => DateTimeOffset.TryParse(rawDate, out var dto)
            ? dto.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
            : rawDate?.Trim() ?? "";

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"&lt;/?[a-zA-Z][^&]*?&gt;")]
    private static partial Regex EncodedHtmlTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
