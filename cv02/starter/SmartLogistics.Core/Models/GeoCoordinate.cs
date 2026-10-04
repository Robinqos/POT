namespace SmartLogistics.Core.Models;

/// <summary>
/// U1: Pripravený malý hodnotový typ. Priradenie kopíruje hodnotu.
/// Struct môže byť uložený aj na halde, napríklad ako člen objektu alebo pri boxingu.
/// Readonly zabraňuje zmene stavu existujúcej hodnoty; with vytvorí novú hodnotu.
/// </summary>
public readonly record struct GeoCoordinate(double Latitude, double Longitude)
{
    /// <summary>Lokálna aproximácia v km, vhodná pre ukážku depa, nie pre navigáciu.</summary>
    public double DistanceTo(GeoCoordinate other)
    {
        double latDiff = (Latitude - other.Latitude) * 111.0;
        double meanLatitude = (Latitude + other.Latitude) / 2.0;
        double lonDiff = (Longitude - other.Longitude) * 111.0
            * Math.Cos(meanLatitude * Math.PI / 180.0);
        return Math.Round(Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff), 2);
    }

    public override string ToString() => $"[{Latitude:F4}, {Longitude:F4}]";
}
