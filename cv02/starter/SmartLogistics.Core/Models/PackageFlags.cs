namespace SmartLogistics.Core.Models;

/// <summary>
/// U1: Pripravené nezávislé bity. V Program.cs pridajte a odoberte príznak.
/// [Flags] podporuje čitateľné formátovanie; správne bitové hodnoty určuje programátor.
/// </summary>
[Flags]
public enum PackageFlags : ushort
{
    None = 0,
    Fragile = 1 << 0,
    Express = 1 << 1,
    Perishable = 1 << 2,
    Heavy = 1 << 3,       // Ručne zadaný manipulačný príznak, neodvodzuje sa z hmotnosti.
    Oversized = 1 << 4,
    RequiresColdChain = 1 << 5
}
