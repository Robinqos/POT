using DevPulse.Core.Models;

namespace DevPulse.Core.Services;

public readonly record struct ArticleMetrics(long SizeBytes, long ElapsedMs, int EstimatedReadingMinutes);

/// <summary>
/// Služba na načítavanie RSS/Atom kanálov a asynchrónnu analýzu článkov.
/// </summary>
public interface IFeedService
{
    IReadOnlyList<FeedSource> GetDefaultSources();
    Task<IReadOnlyList<FeedArticle>> LoadFeedAsync(string url, CancellationToken cancellationToken = default);
    Task<ArticleMetrics> AnalyzeArticleAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchrónne zanalyzuje kolekciu článkov, priebežne hlási postup cez <see cref="IProgress{T}"/>
    /// (prednáška WPF, slajd 17) a podporuje kooperatívne zrušenie cez <see cref="CancellationToken"/>.
    /// </summary>
    Task AnalyzeArticlesAsync(
        IReadOnlyList<FeedArticle> articles,
        IProgress<AnalysisProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
