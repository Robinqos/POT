using SmartLogistics.Core.Models;

namespace SmartLogistics.Core.Vehicles;

/// <summary>
/// U3b: Zapečatená trieda (sealed class).
/// 
/// Využite GitHub Copilot:
/// Trieda je už sealed. Preskúmajte, čo toto kľúčové slovo zakazuje.
/// Prekryte metódu CalculateCost (cena je 1.50m + distanceKm * 0.15m).
/// </summary>
public sealed class ElectricCargoBike(string serialNumber)
    : DeliveryVehicle(serialNumber, 40.0)
{
    // TODO: U3b - Doplňte CalculateCost; Deliver je pripravený.
    public override decimal CalculateCost(double distanceKm)
    {
        return 1.50m + (decimal)distanceKm * 0.15m;
    }

    public override void Deliver(Package package)
    {
        Console.WriteLine($"[E-BIKE {LicensePlate}] Eko doručenie v pešej zóne: Balík {package.TrackingNumber}.");
    }

    public override string GetDiagnostics() =>
        $"[E-BIKE] Sériové číslo: {LicensePlate}, Zapečatená kategória 'Last-Mile Eco'.";
}
