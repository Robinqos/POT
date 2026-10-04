namespace SmartLogistics.Core.Models;

/// <summary>U2: Pozičný record class s init vlastnosťami a hodnotovou rovnosťou.
/// With robí plytkú kópiu; record môže obsahovať aj meniteľné členy.
/// Porovnajte s LegacyPackage, hotový Package neprepisujte druhým typom.</summary>
public record class Package(
    string TrackingNumber,
    GeoCoordinate Destination,
    PackageFlags Flags,
    decimal BasePrice,
    double WeightKg)
{
    public const decimal DefaultInsuranceRate = 0.02m;
    public decimal InsuranceFee => BasePrice * DefaultInsuranceRate;

    /// <summary>
    /// TODO: U2 - Implementujte vlastnú Deconstruct metódu.
    /// Táto metóda umožňuje dekonštrukciu: var (code, price) = package;
    /// </summary>
    public void Deconstruct(out string trackingNumber, out decimal basePrice)
    {
        // TODO: Nastavte výstupné parametre trackingNumber a basePrice
        throw new NotImplementedException("U2: Implementujte vlastnú Deconstruct metódu.");
    }
}
