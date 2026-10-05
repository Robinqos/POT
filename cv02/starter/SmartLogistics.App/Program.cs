using System.Globalization;
using SmartLogistics.Core.Extensions;
using SmartLogistics.Core.Interfaces;
using SmartLogistics.Core.Legacy;
using SmartLogistics.Core.Models;
using SmartLogistics.Core.Security;
using SmartLogistics.Core.Services;
using SmartLogistics.Core.Vehicles;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sk-SK");
Console.OutputEncoding = System.Text.Encoding.UTF8;
int selectedUnit = 0;
if (args.Length != 0 && (args.Length != 2 || args[0] != "--unit"
    || !int.TryParse(args[1], out selectedUnit) || selectedUnit is < 1 or > 6))
{
    Console.Error.WriteLine("Použitie: dotnet run --project SmartLogistics.App -- [--unit 1 až 6]");
    return 2;
}
int pending = 0, errors = 0;
Console.WriteLine("SMART LOGISTICS  |  POT laboratórne cvičenie 2\n");

RunUnit(1, "Hodnotové typy a príznaky", () =>
{
    var origin = new GeoCoordinate(49.2231, 18.7394);
    var copy = origin;
    Console.WriteLine($"Súradnice pred experimentom: {origin}, kópia: {copy}");
    // EXPERIMENT U1: Za tento komentár pridajte copy = copy with { Latitude = 48.0 };
    copy = copy with { Latitude = 48.0 };
    //with vytvorí novú hodnotu (record struct), origin sa nezmení.
    Console.WriteLine($"Súradnice po experimente: pôvodné {origin}, kópia {copy}");
    Console.WriteLine($"Vzdialenosť: {origin.DistanceTo(new(49.2100, 18.7500))} km");
    var flags = PackageFlags.Fragile | PackageFlags.Express;
    Console.WriteLine($"Príznaky pred experimentom: {flags}, číselne {(ushort)flags}");
    // EXPERIMENT U1: Pridajte Heavy pomocou |=, odoberte Fragile pomocou &= ~.
    flags |= PackageFlags.Heavy;
    flags &= ~PackageFlags.Fragile;
    //toto zmení hodnotu flags, ale pôvodné príznaky sa nezmenia.
    // |= pridá, &= ~ odoberie.
    //None kontrola: flags == None je False; HasFlag(None) je True
    Console.WriteLine($"Príznaky po experimente: {flags}, číselne {(ushort)flags}");
    Console.WriteLine($"None kontrola: flags == None je {flags == PackageFlags.None}; HasFlag(None) je {flags.HasFlag(PackageFlags.None)}");
});

RunUnit(2, "Rekordy a dekonštrukcia", () =>
{
    var original = Create("SK-ZA-1001", price: 10m);
    var alias = original;
    var upgraded = original with { BasePrice = 15m };
    Console.WriteLine($"Priradenie record class zdieľa objekt: {ReferenceEquals(original, alias)}");
    Console.WriteLine($"With zdieľa objekt: {ReferenceEquals(original, upgraded)}");
    Console.WriteLine($"Ceny: pôvodná {original.BasePrice}, nová {upgraded.BasePrice}, poistenie {original.InsuranceFee}");
    Console.WriteLine($"Dva samostatné rekordy s rovnakými dátami: {original == Create("SK-ZA-1001", price: 10m)}");
    var legacy = new LegacyPackage("OLD", 10m, 2);
    var legacyAlias = legacy;
    legacyAlias.BasePrice = 20m;
    Console.WriteLine($"Meniteľná trieda cez druhú referenciu: pôvodná cena je teraz {legacy.BasePrice}");
    var (code, price) = original;
    Console.WriteLine($"Deconstruct: {code}, {price}");
    // EXPERIMENT U2: Skúste original.BasePrice = 99m; preložte a potom riadok zakomentujte.
    //original.BasePrice = 99m;

    //dostal som chybu: 
    //Init - only property or indexer 'Package.BasePrice'
    //can only be assigned in an object initializer,
    //or on 'this' or 'base' in an instance constructor
    //or an 'init' accessor.

    /*
     * Package je pozičný record – kompilátor z parametrov BasePrice 
     * vygeneroval init-only vlastnosť. Priradiť sa dá len pri vytvorení
     * objektu (v inicializéri) alebo cez with. Po vytvorení sa už meniť
     * nedá – to je presne tá „nemennosť", ktorú record podporuje 
     * (ale nie garantuje úplne, ak by tam boli set vlastnosti).
     * zmenit cenu sa da cez : var drahsi = original with { BasePrice = 99m };
     */
});

RunUnit(3, "Dedičnosť a polymorfizmus", () =>
{
    DeliveryVehicle van = new DeliveryVan("ZA-VAN", 1000, isElectric: true);
    DeliveryVehicle bike = new ElectricCargoBike("BIKE");
    var drone = new DroneDelivery("DRONE");
    DeliveryVehicle droneAsVehicle = drone;
    Console.WriteLine($"Dodávka na 10 km: {van.CalculateCost(10)}; bicykel: {bike.CalculateCost(10)}");
    Console.WriteLine($"Dron cez DroneDelivery: {drone.GetDiagnostics()}");
    Console.WriteLine($"Ten istý dron cez DeliveryVehicle: {droneAsVehicle.GetDiagnostics()}");
    foreach (DeliveryVehicle vehicle in new DeliveryVehicle[] {van, bike, drone})
        vehicle.Deliver(Create());
    // EXPERIMENT U3: Bod prerušenia na CalculateCost v DeliveryVan; sledujte baseCost a IsElectric.
});

RunUnit(4, "Rozhrania a prístup k členom", () =>
{
    var tracked = new TrackedPackage(Create(), "Žilina");
    tracked.UpdateLocation("Martin");
    Console.WriteLine(tracked.GetStatus());
    ITrackable trackable = tracked;
    Console.WriteLine(trackable.Ping());
    Console.WriteLine(((ISecureAuditable)tracked).GetAuditRecord());
    Console.WriteLine(new LocalFleetSubSecurity().DescribeAccess());
    // EXPERIMENT U4: Skúšajte po jednom, po zistení chyby znovu zakomentujte:
    //tracked.CurrentLocation = "Obídená história"; // private set
    /*
     * The property or indexer 'TrackedPackage.CurrentLocation' cannot be used 
     * in this context because the set accessor is inaccessible
     */

    //tracked.GetAuditRecord(); // volanie vyžaduje typ rozhrania
    /*
     * 'TrackedPackage' does not contain a definition for 'GetAuditRecord'
     * and no accessible extension method 'GetAuditRecord' accepting a 
     * first argument of type 'TrackedPackage' could be found 
     * (are you missing a using directive or an assembly reference?)
     */
    // tracked.Ping(); // default implementácia vyžaduje typ rozhrania
    /*
     * 'TrackedPackage' does not contain a definition for 'Ping' and 
     * no accessible extension method 'Ping' accepting a first argument of 
     * type 'TrackedPackage' could be found 
     * (are you missing a using directive or an assembly reference?)
     */
    // Console.WriteLine(new FleetSecurityBase().DepotEncryptionKey); // internal v inom zostavení
    /*
     * RECAP:
tracked.CurrentLocation = "Obídená história";
Chyba: CS0272: The property or indexer 'TrackedPackage.CurrentLocation' cannot be used in this context because the set accessor is inaccessible.
Vlastnosť má private set, takže mimo triedy sa nastaviť nedá. Správne sa používa UpdateLocation("..."), ktorá aktualizuje aj históriu.
tracked.GetAuditRecord();
Chyba: CS1061: 'TrackedPackage' does not contain a definition for 'GetAuditRecord'.
Explicitná implementácia nie je členom triedy – existuje len cez rozhranie.
tracked.Ping();
Chyba: CS1061: 'TrackedPackage' does not contain a definition for 'Ping'.
Ping je default interface member v ITrackable. Volať sa dá len cez premennú typu ITrackable (napr. ITrackable t = tracked; t.Ping();).
Console.WriteLine(new FleetSecurityBase().DepotEncryptionKey);
Chyba: CS0122: 'FleetSecurityBase.DepotEncryptionKey' is inaccessible due to its protection level.
Vlastnosť je internal, viditeľná len v zostavení SmartLogistics.Core. Program.cs je v zostavení SmartLogistics.App – iné zostavenie, preto neprístupná.
     */
});

RunUnit(5, "Depo a metódy", () =>
{
    var depot = new Depot("Sever", "SK-ZA-");
    depot.Add(Create("SK-ZA-1001", price: 10m, weight: 2), Create("SK-ZA-1002", price: 20m, weight: 3));
    Console.WriteLine($"Indexer int: {depot[0].TrackingNumber}; string: {depot["sk-za-1002"]?.BasePrice}");
    Console.WriteLine($"Neznámy kód: {depot["UNKNOWN"]?.TrackingNumber ?? "nenájdený"}");
    var (count, value, weight) = depot.GetStatistics();
    Console.WriteLine($"Štatistiky pred registráciou: {count}, {value}, {weight}");
    Console.WriteLine($"Platný balík: {depot.RegisterWithValidation(Create("SK-ZA-1003"))}");
    Console.WriteLine($"Zlý prefix: {depot.RegisterWithValidation(Create("CZ-ZA-1004"), minCodeLength: 7)}");
    Package a = depot[0], b = depot[1];
    Depot.Swap(ref a, ref b);
    Console.WriteLine($"Swap lokálnych premenných: A={a.TrackingNumber}, B={b.TrackingNumber}; depo[0]={depot[0].TrackingNumber}");
    foreach (var package in depot) Console.WriteLine(package.TrackingNumber);
});

RunUnit(6, "Dispečing a rozšírenia", () =>
{
    (Package package, double km)[] deliveries =
    [
        (Create("COLD", PackageFlags.RequiresColdChain | PackageFlags.Heavy, weight: 30), 70),
        (Create("CITY"), 3),
        (Create("HEAVY", PackageFlags.Heavy), 3),
        (Create("LONG"), 70),
        (Create("STANDARD"), 20)
    ];
    foreach (var (package, km) in deliveries)
    {
        var (vehicle, price) = DispatcherService.Dispatch(package, km);
        Console.WriteLine($"{package.TrackingNumber}: {vehicle}, {price}");
    }
    Package[] packages = [Create("A", PackageFlags.Express, 15m),
        Create("B", PackageFlags.Express | PackageFlags.Fragile, 25m), Create("C", price: 10m)];
    Console.WriteLine($"Expresná hodnota: {packages.TotalExpressValue()}");
    //var manifest = packages.Select(p => new {p.TrackingNumber, IsUrgent = p.Flags.HasFlag(PackageFlags.Express)});
    //foreach (var item in manifest) Console.WriteLine($"{item.TrackingNumber}: súrne={item.IsUrgent}");
    // EXPERIMENT U6: Do anonymného typu pridajte p.WeightKg a vypíšte novú vlastnosť.
    var manifest = packages.Select(p => new
    {
        p.TrackingNumber,
        p.WeightKg,
        IsUrgent = p.Flags.HasFlag(PackageFlags.Express)
    });
    foreach (var item in manifest)
        Console.WriteLine($"{item.TrackingNumber}: súrne={item.IsUrgent}, váha={item.WeightKg}");

});

Console.WriteLine($"\nNedokončené bloky: {pending}; neočakávané chyby: {errors}.");
Console.WriteLine("Ak sú všetky bloky hotové, spustite aj testy. Výpis sám neoverí všetky hranice.");
return errors == 0 ? 0 : 1;

void RunUnit(int number, string title, Action action)
{
    if (selectedUnit != 0 && selectedUnit != number) return;
    Console.WriteLine($"\nU{number}  {title}");
    try { action(); }
    catch (NotImplementedException ex) { pending++; Console.WriteLine($"ČAKÁ NA DOPLNENIE: {ex.Message}"); }
    catch (Exception ex) { errors++; Console.Error.WriteLine($"CHYBA: {ex.GetType().Name}: {ex.Message}"); }
}

static Package Create(string code = "SK-ZA-1001", PackageFlags flags = PackageFlags.None,
    decimal price = 10m, double weight = 2) => new(code, new(49, 18), flags, price, weight);
