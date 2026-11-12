using CommunityToolkit.Mvvm.ComponentModel;
using DevPulse.Core.Services;

namespace DevPulse.Core.Models;

/// <summary>
/// Položka článku načítaná z RSS/Atom zdroja.
/// Dedí od ObservableObject, aby sa výsledky asynchrónnej analýzy (odozva, veľkosť, čas čítania)
/// okamžite prejavili v UI bez nutnosti prekresľovať celý zoznam.
/// </summary>
public partial class FeedArticle : ObservableObject
{
    public required string Title { get; init; }
    public required string Link { get; init; }
    public string Summary { get; init; } = "";
    public string PublishedText { get; init; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetricsSummary))]
    private bool isAnalyzed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetricsSummary))]
    private long responseTimeMs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetricsSummary))]
    private double sizeKiB;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetricsSummary))]
    private int readingTimeMinutes;

    public string MetricsSummary => IsAnalyzed
        ? $"{ResponseTimeMs} ms  ·  {SizeKiB:F1} KiB  ·  ~{ReadingTimeMinutes} min čítania"
        : "Čaká na analýzu odozvy";

    public void ApplyMetrics(ArticleMetrics metrics)
    {
        ResponseTimeMs = metrics.ElapsedMs;
        SizeKiB = Math.Round(metrics.SizeBytes / 1024.0, 1);
        ReadingTimeMinutes = metrics.EstimatedReadingMinutes;
        IsAnalyzed = true;
    }
}
