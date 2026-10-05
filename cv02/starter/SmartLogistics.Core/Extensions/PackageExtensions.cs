using SmartLogistics.Core.Models;

namespace SmartLogistics.Core.Extensions;

/// <summary>
/// U6b: Rozširujúce metódy (Extension Methods).
/// 
/// Využite GitHub Copilot:
/// Napíšte komentár:
/// // Vytvor rozširujúcu metódu TotalExpressValue pre IEnumerable<Package>, ktorá spočíta BasePrice všetkých balíkov s príznakom Express.
/// </summary>
public static class PackageExtensions
{
    public static string ToShippingLabel(this Package package)
    {
        return $"[ZÁSIELKA: {package.TrackingNumber}] Cieľ: {package.Destination}, " +
               $"Váha: {package.WeightKg:F1}kg, Príznaky: [{package.Flags}], " +
               $"Cena: {package.BasePrice:C2} (Poistenie: {package.InsuranceFee:C2})";
    }

    // TODO: U6b - Implementujte rozširujúcu metódu TotalExpressValue
    public static decimal TotalExpressValue(this IEnumerable<Package> packages)
    {
        return packages
            .Where(p => p.Flags.HasFlag(PackageFlags.Express))
            .Sum(p => p.BasePrice);
    }
}
