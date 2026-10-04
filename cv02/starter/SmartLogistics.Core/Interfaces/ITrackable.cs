namespace SmartLogistics.Core.Interfaces;

/// <summary>
/// Rozhranie pre sledovateľné objekty.
/// Demonštruje predvolenú (default) implementáciu metódy v rozhraní (C# 8+).
/// </summary>
public interface ITrackable
{
    string GetStatus();

    /// <summary>
    /// Predvolená implementácia člena rozhrania (Default Interface Member).
    /// Trieda ju nemusí implementovať. Volanie je dostupné cez premennú typu ITrackable.
    /// </summary>
    public string Ping() => "[TELEMETRIA] Zariadenie/balík odpovedá na signál.";
}
