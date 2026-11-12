namespace DevPulse.Core.Models;

/// <summary>
/// Definícia zdroja článkov (RSS / Atom XML alebo vstavaný offline zdroj).
/// </summary>
public sealed record FeedSource(string Name, string Url)
{
    public override string ToString() => Name;
}

/// <summary>
/// Priebežný stav asynchrónnej analýzy odkazov pre IProgress&lt;AnalysisProgress&gt; (prednáška WPF, slajd 17).
/// </summary>
public readonly record struct AnalysisProgress(int Completed, int Total, string CurrentTitle)
{
    public double Percent => Total == 0 ? 0 : Math.Round((double)Completed / Total * 100.0, 1);
}
