using SmartLogistics.Core.Models;

namespace SmartLogistics.Core.Vehicles;

/// <summary>
/// U3c: Skrytie metódy (Method Hiding) pomocou 'new'.
/// 
/// Využite GitHub Copilot:
/// Implementujte metódu GetDiagnostics(), ale namiesto 'override' použite kľúčové slovo 'new'.
/// Zistite, čo sa stane, keď referenciu na DroneDelivery priradíte do premennej typu DeliveryVehicle.
/// </summary>
public class DroneDelivery(string droneCode)
    : DeliveryVehicle(droneCode, 4.5)
{
    public override decimal CalculateCost(double distanceKm) => 3.50m + ((decimal)distanceKm * 0.40m);

    public override void Deliver(Package package)
    {
        Console.WriteLine($"[DRON {LicensePlate}] Letecké doručenie balíka {package.TrackingNumber}.");
    }

    // TODO: U3c - Použite kľúčové slovo 'new' na skrytie zdedenej metódy GetDiagnostics()
    public new string GetDiagnostics()
    {
        return $"[DRON ŠPECIFICKÉ] Kód: {LicensePlate}, Firmware: v3.1, Letová hladina: 120m AGL.";
    }
}
