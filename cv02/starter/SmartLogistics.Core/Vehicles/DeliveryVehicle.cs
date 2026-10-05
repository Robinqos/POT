using SmartLogistics.Core.Models;

namespace SmartLogistics.Core.Vehicles;

/// <summary>
/// Abstraktná základná trieda pre všetky vozidlá logistického parku.
/// Demonštruje:
/// 1. Primárny konštruktor triedy v C# 12.
/// 2. Abstraktnú triedu a metódy (abstract).
/// 3. Virtuálne metódy (virtual) a dedičnosť.
/// </summary>
public abstract class DeliveryVehicle(string licensePlate, double maxCapacityKg)
{
    public string LicensePlate { get; init; } = licensePlate;
    public double MaxCapacityKg { get; init; } = maxCapacityKg;

    // Virtuálna metóda: V C# metódy nie sú virtuálne implicitne (na rozdiel od Javy).
    // Musíme použiť kľúčové slovo 'virtual', aby ju potomkovia mohli prekryť cez 'override'.
    public virtual decimal CalculateCost(double distanceKm)
    {
        const decimal baseFee = 2.00m;
        const decimal costPerKm = 0.60m;
        return baseFee + ((decimal)distanceKm * costPerKm);
    }

    // Abstraktná metóda: Nemá telo, odvodená trieda ju MUSÍ implementovať.
    public abstract void Deliver(Package package);

    // Virtuálna metóda pre diagnostiku – bude prekrytá alebo skrytá pomocou 'new'
    public virtual string GetDiagnostics() => $"[VOZIDLO] ŠPZ: {LicensePlate}, Nosnosť: {MaxCapacityKg} kg";
}
