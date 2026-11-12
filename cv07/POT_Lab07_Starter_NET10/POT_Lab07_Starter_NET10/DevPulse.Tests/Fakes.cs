using DevPulse.Core.Models;
using DevPulse.Core.Services;

namespace DevPulse.Tests;

/// <summary>
/// Falošná implementácia dialógovej služby pre unit testy (prednáška MVVM, slajd 19).
/// Umožňuje simulovať kliknutie používateľa na „Áno" alebo „Zrušiť" bez otvorenia okna.
/// </summary>
internal sealed class FakeDialogService : IDialogService
{
    public bool NextConfirmationResult { get; set; } = true;
    public int ConfirmCallsCount { get; private set; }
    public string? LastInfoMessage { get; private set; }

    public Task ShowInfoAsync(string title, string message)
    {
        LastInfoMessage = message;
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string title, string message)
    {
        ConfirmCallsCount++;
        return Task.FromResult(NextConfirmationResult);
    }
}

/// <summary>
/// Falošná implementácia IFeedService pre rýchle, deterministické a offline unit testy.
/// </summary>
internal sealed class FakeFeedService : IFeedService
{
    public TimeSpan SimulatedAnalysisDelay { get; set; } = TimeSpan.Zero;

    public List<FeedArticle> ArticlesToReturn { get; set; } =
    [
        new() { Title = "Testovací článok 1", Link = "https://example.com/1", Summary = "Ukážka 1" },
        new() { Title = "Testovací článok 2", Link = "https://example.com/2", Summary = "Ukážka 2" }
    ];

    public IReadOnlyList<FeedSource> GetDefaultSources() =>
    [
        new("Test RSS", "https://example.com/rss")
    ];

    public Task<IReadOnlyList<FeedArticle>> LoadFeedAsync(string url, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<FeedArticle>>(ArticlesToReturn.ToList());
    }

    public async Task<ArticleMetrics> AnalyzeArticleAsync(string url, CancellationToken cancellationToken = default)
    {
        if (SimulatedAnalysisDelay > TimeSpan.Zero)
        {
            await Task.Delay(SimulatedAnalysisDelay, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new ArticleMetrics(SizeBytes: 24_576, ElapsedMs: 42, EstimatedReadingMinutes: 4);
    }

    public async Task AnalyzeArticlesAsync(
        IReadOnlyList<FeedArticle> articles,
        IProgress<AnalysisProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < articles.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var article = articles[i];
            var metrics = await AnalyzeArticleAsync(article.Link, cancellationToken);
            article.ApplyMetrics(metrics);
            progress?.Report(new AnalysisProgress(i + 1, articles.Count, article.Title));
        }
    }
}

/// <summary>
/// Falošný spúšťač prehliadača – zaznamená otvorenú URL namiesto spúšťania reálneho prehliadača počas testov.
/// </summary>
internal sealed class FakeBrowserLauncher : IBrowserLauncher
{
    public string? LastOpenedUrl { get; private set; }

    public void OpenUrl(string url) => LastOpenedUrl = url;
}
