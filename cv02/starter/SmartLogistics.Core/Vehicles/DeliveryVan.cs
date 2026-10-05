using SmartLogistics.Core.Models;

namespace SmartLogistics.Core.Vehicles;

/// <summary>
/// U3a: Dodávka – dedičnosť a 'override'.
/// 
/// Využite GitHub Copilot:
/// Prekryte metódu CalculateCost(double distanceKm) tak, aby zavolala implementáciu predka cez base.CalculateCost(distanceKm).
/// Ak je IsElectric true, znížte cenu o 25 % (* 0.75m), inak zvýšte o 15 % (* 1.15m).
/// </summary>
public class DeliveryVan(string licensePlate, double maxCapacityKg, bool isElectric)
    : DeliveryVehicle(licensePlate, maxCapacityKg)
{
    public bool IsElectric { get; } = isElectric;

    // TODO: U3a - Prekryte metódu CalculateCost pomocou kľúčového slova 'override' a volania 'base.CalculateCost'
    public override decimal CalculateCost(double distanceKm)
    {
        decimal baseCost = base.CalculateCost(distanceKm);
        return IsElectric ? baseCost * 0.75m : baseCost * 1.15m;
    }

    public override void Deliver(Package package)
    {
        Console.WriteLine($"[DODÁVKA {LicensePlate}] Doručený balík {package.TrackingNumber} pre cieľ {package.Destination}.");
    }

    public override string GetDiagnostics() =>
        $"{base.GetDiagnostics()} | Pohon: {(IsElectric ? "Elektromobil" : "Diesel")}";
}
