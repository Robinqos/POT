using SmartLogistics.Core.Models;

namespace SmartLogistics.Core.Services;

/// <summary>U6a: Prvá zhodná vetva vyhráva. Poradie je súčasťou zadania.</summary>
public static class DispatcherService
{
    // Platné vstupy: distanceKm konečné a >= 0, WeightKg konečné a > 0, BasePrice >= 0.
    // Pri porušení vyvolajte ArgumentOutOfRangeException pre príslušný parameter.
    public static (string VehicleType, decimal FinalPrice) Dispatch(Package package, double distanceKm)
    {
        ArgumentNullException.ThrowIfNull(package);
        if (!double.IsFinite(distanceKm) || distanceKm < 0)
            throw new ArgumentOutOfRangeException(nameof(distanceKm));
        if (!double.IsFinite(package.WeightKg) || package.WeightKg <= 0 || package.BasePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(package));

        // U6a: 1. ColdChain -> chladiarenská dodávka x1.50
        // 2. <=5 km, <=8 kg, bez Heavy/Oversized -> elektrobicykel x0.90
        // 3. >20 kg alebo Heavy/Oversized -> ťažká nákladná dodávka x1.35
        // 4. >60 km -> diaľkový kamión x1.25
        // 5. Inak -> štandardná dodávka x1.00
        // TODO U6a: Doplňte switch výraz podľa pravidiel vyššie.
        throw new NotImplementedException("U6a: Switch výraz dispečingu");
    }
}
