using SmartLogistics.Core.Extensions;
using SmartLogistics.Core.Interfaces;
using SmartLogistics.Core.Legacy;
using SmartLogistics.Core.Models;
using SmartLogistics.Core.Services;
using SmartLogistics.Core.Vehicles;
using Xunit;

namespace SmartLogistics.Tests;

// Rovnaké akceptačné testy sú v starter aj solution. Červené TODO testy sú na začiatku očakávané.
public class SmartLogisticsTests
{
    private static Package Create(string code = "SK-ZA-1001", PackageFlags flags = PackageFlags.None,
        decimal price = 10m, double weight = 2) => new(code, new(49, 18), flags, price, weight);

    [Fact, Trait("Unit", "U1"), Trait("Level", "Smoke")]
    public void U1_ValueCopy_WithLeavesOriginalCoordinateUnchanged()
    {
        var original = new GeoCoordinate(49, 18);
        var copy = original;
        copy = copy with { Latitude = 48 };
        Assert.True(typeof(GeoCoordinate).IsValueType);
        Assert.Equal(49, original.Latitude);
        Assert.Equal(48, copy.Latitude);
    }

    [Fact, Trait("Unit", "U1"), Trait("Level", "Smoke")]
    public void U1_Distance_IsZeroForSamePoint_AndSymmetric()
    {
        var a = new GeoCoordinate(49.2231, 18.7394);
        var b = new GeoCoordinate(49.2100, 18.7500);
        Assert.Equal(0.0, a.DistanceTo(a));
        Assert.Equal(a.DistanceTo(b), b.DistanceTo(a));
        Assert.InRange(a.DistanceTo(b), 1.5, 1.8);
    }

    [Fact, Trait("Unit", "U1"), Trait("Level", "Smoke")]
    public void U1_Flags_AreIndependentBits_WithExpectedValues()
    {
        Assert.Equal(new ushort[] {0, 1, 2, 4, 8, 16, 32},
            Enum.GetValues<PackageFlags>().Select(f => (ushort)f).ToArray());
        Assert.True(typeof(PackageFlags).IsDefined(typeof(FlagsAttribute), false));
    }

    [Fact, Trait("Unit", "U1"), Trait("Level", "Smoke")]
    public void U1_Flags_AddAndRemoveDoNotChangeOtherBits()
    {
        var flags = PackageFlags.Fragile | PackageFlags.Express;
        flags |= PackageFlags.Heavy;
        flags &= ~PackageFlags.Fragile;
        Assert.Equal(PackageFlags.Express | PackageFlags.Heavy, flags);
        Assert.False(flags.HasFlag(PackageFlags.Perishable));
        Assert.True(flags.HasFlag(PackageFlags.None)); // None kontrolujte cez flags == None.
    }

    [Fact, Trait("Unit", "U2"), Trait("Level", "Smoke")]
    public void U2_Record_EqualityUsesValues_AssignmentCopiesReference()
    {
        var original = Create();
        var alias = original;
        var equal = Create();
        Assert.Same(original, alias);
        Assert.NotSame(original, equal);
        Assert.Equal(original, equal);
        var legacy = new LegacyPackage("A", 10m, 2);
        var legacyAlias = legacy;
        legacyAlias.BasePrice = 20m;
        Assert.Equal(20m, legacy.BasePrice);
    }

    [Fact, Trait("Unit", "U2"), Trait("Level", "Smoke")]
    public void U2_With_CreatesNewObject_AndPreservesOriginal()
    {
        var original = Create();
        var modified = original with { BasePrice = 15m };
        Assert.NotSame(original, modified);
        Assert.Equal(10m, original.BasePrice);
        Assert.Equal(15m, modified.BasePrice);
        Assert.Equal(original.TrackingNumber, modified.TrackingNumber);
    }

    [Fact, Trait("Unit", "U2"), Trait("Level", "Smoke")]
    public void U2_Insurance_UsesDecimalAndConstRate() =>
        Assert.Equal(0.20m, Create().InsuranceFee);

    [Fact, Trait("Unit", "U2"), Trait("Level", "Core")]
    public void U2_Deconstruct_ReturnsTrackingCodeAndBasePrice()
    {
        var (code, price) = Create("SK-ZA-2001", price: 24.50m);
        Assert.Equal("SK-ZA-2001", code);
        Assert.Equal(24.50m, price);
    }

    [Theory, InlineData(true, 6.00), InlineData(false, 9.20)]
    [Trait("Unit", "U3"), Trait("Level", "Core")]
    public void U3_Van_UsesOverrideAndBaseCost(bool electric, double expected)
    {
        DeliveryVehicle vehicle = new DeliveryVan("VAN", 1000, electric);
        Assert.Equal((decimal)expected, vehicle.CalculateCost(10));
    }

    [Fact, Trait("Unit", "U3"), Trait("Level", "Core")]
    public void U3_Bike_IsSealed_AndCostIsExact()
    {
        DeliveryVehicle vehicle = new ElectricCargoBike("BIKE");
        Assert.True(typeof(ElectricCargoBike).IsSealed);
        Assert.Equal(3.00m, vehicle.CalculateCost(10));
    }

    [Fact, Trait("Unit", "U3"), Trait("Level", "Core")]
    public void U3_New_UsesStaticType_WhileOverrideUsesRuntimeType()
    {
        var drone = new DroneDelivery("DRONE");
        DeliveryVehicle baseReference = drone;
        Assert.Contains("[DRON ŠPECIFICKÉ]", drone.GetDiagnostics());
        Assert.Contains("[VOZIDLO]", baseReference.GetDiagnostics());
        Assert.Equal(7.50m, baseReference.CalculateCost(10));
    }

    [Fact, Trait("Unit", "U4"), Trait("Level", "Smoke")]
    public void U4_DefaultInterfaceMember_IsCallableThroughInterface()
    {
        ITrackable trackable = new TrackedPackage(Create(), "Žilina");
        Assert.Contains("[TELEMETRIA]", trackable.Ping());
        Assert.Null(typeof(TrackedPackage).GetMethod("Ping"));
    }

    [Fact, Trait("Unit", "U4"), Trait("Level", "Core")]
    public void U4_Audit_IsExplicit_AndIncludesLocationHistory()
    {
        var tracked = new TrackedPackage(Create(), "Žilina");
        tracked.UpdateLocation("Martin");
        Assert.Contains("Martin", tracked.GetStatus());
        Assert.False(typeof(TrackedPackage).GetProperty("CurrentLocation")!.SetMethod!.IsPublic);
        Assert.Null(typeof(TrackedPackage).GetMethod("GetAuditRecord"));
        string audit = ((ISecureAuditable)tracked).GetAuditRecord();
        Assert.StartsWith("[INTERNÝ AUDIT]", audit);
        Assert.Contains("SK-ZA-1001", audit);
        Assert.Contains("Žilina", audit);
        Assert.Contains("Martin", audit);
    }

    [Fact, Trait("Unit", "U5"), Trait("Level", "Smoke")]
    public void U5_Params_IntIndexerSetter_AndForeach_Work()
    {
        var depot = new Depot("Depo", "SK-ZA-");
        depot.Add();
        depot.Add(Create("A"), Create("B"));
        depot[0] = Create("C");
        Assert.Equal(new[] { "C", "B" }, depot.Select(p => p.TrackingNumber).ToArray());
        Assert.Throws<ArgumentOutOfRangeException>(() => depot[-1]);
    }

    [Fact, Trait("Unit", "U5"), Trait("Level", "Core")]
    public void U5_StringIndexer_IsCaseInsensitive_AndMissingIsNull()
    {
        var depot = new Depot("Depo", "SK-ZA-");
        var first = Create();
        depot.Add(first, Create());
        Assert.Same(first, depot["sk-za-1001"]);
        Assert.Null(depot["UNKNOWN"]);
    }

    [Theory]
    [InlineData("SK-ZA-1", 0.01, false)]
    [InlineData("SK-ZA-1", 0.02, true)]
    [InlineData("sk-za-1", 100.0, true)]
    [InlineData("SK-ZA-1", 100.01, false)]
    [InlineData("CZ-ZA-1", 2.0, false)]
    [InlineData("SK-ZA-", 2.0, false)]
    [Trait("Unit", "U5"), Trait("Level", "Core")]
    public void U5_Validation_RespectsPrefixLengthWeight_AndOnlyAddsValid(string code, double weight, bool expected)
    {
        var depot = new Depot("Depo", "SK-ZA-");
        Assert.Equal(expected, depot.RegisterWithValidation(Create(code, weight: weight), minCodeLength: 7));
        Assert.Equal(expected ? 1 : 0, depot.Count);
    }

    [Fact, Trait("Unit", "U5"), Trait("Level", "Core")]
    public void U5_Validation_RejectsNonFiniteWeight()
    {
        var depot = new Depot("Depo", "SK-ZA-");
        Assert.False(depot.RegisterWithValidation(Create(weight: double.NaN)));
        Assert.False(depot.RegisterWithValidation(Create(weight: double.PositiveInfinity)));
        Assert.Equal(0, depot.Count);
    }

    [Fact, Trait("Unit", "U5"), Trait("Level", "Core")]
    public void U5_Statistics_EmptyAndPopulatedDepotHaveCorrectTotals()
    {
        var depot = new Depot("Depo", "SK-ZA-");
        Assert.Equal((0, 0m, 0.0), depot.GetStatistics());
        depot.Add(Create("A", price: 10m, weight: 2), Create("B", price: 20m, weight: 3));
        var (count, value, weight) = depot.GetStatistics();
        Assert.Equal(2, count);
        Assert.Equal(30m, value);
        Assert.Equal(5.0, weight);
    }

    [Fact, Trait("Unit", "U5"), Trait("Level", "Smoke")]
    public void U5_GenericSwap_WorksForValueAndReferenceVariables()
    {
        int a = 1, b = 2;
        Depot.Swap(ref a, ref b);
        Assert.Equal((2, 1), (a, b));
        var depot = new Depot("Depo", "SK-ZA-");
        var first = Create("A");
        var second = Create("B");
        depot.Add(first, second);
        Depot.Swap(ref first, ref second);
        Assert.Equal("B", first.TrackingNumber);
        Assert.Equal("A", depot[0].TrackingNumber); // Referencie v depe sa neprehodili.
    }

    [Theory]
    [InlineData(PackageFlags.RequiresColdChain | PackageFlags.Heavy, 70, 30, "Chladiarenská dodávka", 15.0)]
    [InlineData(PackageFlags.RequiresColdChain, 3, 2, "Chladiarenská dodávka", 15.0)]
    [InlineData(PackageFlags.None, 5, 8, "Elektrobicykel", 9.0)]
    [InlineData(PackageFlags.Express | PackageFlags.Fragile, 3, 2, "Elektrobicykel", 9.0)]
    [InlineData(PackageFlags.None, 5.01, 8, "Štandardná dodávka", 10.0)]
    [InlineData(PackageFlags.None, 5, 8.01, "Štandardná dodávka", 10.0)]
    [InlineData(PackageFlags.Heavy, 3, 2, "Ťažká nákladná dodávka", 13.5)]
    [InlineData(PackageFlags.Oversized, 70, 2, "Ťažká nákladná dodávka", 13.5)]
    [InlineData(PackageFlags.None, 10, 20, "Štandardná dodávka", 10.0)]
    [InlineData(PackageFlags.None, 10, 20.01, "Ťažká nákladná dodávka", 13.5)]
    [InlineData(PackageFlags.None, 60, 2, "Štandardná dodávka", 10.0)]
    [InlineData(PackageFlags.None, 60.01, 2, "Diaľkový kamión", 12.5)]
    [InlineData(PackageFlags.None, 0, 2, "Elektrobicykel", 9.0)]
    [Trait("Unit", "U6"), Trait("Level", "Core")]
    public void U6_Dispatch_UsesOrderedRules_AndExactPrice(PackageFlags flags, double km, double kg, string vehicle, double price)
    {
        var result = DispatcherService.Dispatch(Create(flags: flags, weight: kg), km);
        Assert.Equal(vehicle, result.VehicleType);
        Assert.Equal((decimal)price, result.FinalPrice);
    }

    [Fact, Trait("Unit", "U6"), Trait("Level", "Smoke")]
    public void U6_Dispatch_RejectsInvalidInputs_BeforeApplyingRules()
    {
        Assert.Throws<ArgumentNullException>(() => DispatcherService.Dispatch(null!, 0));
        foreach (double distance in new[] { -1.0, double.NaN, double.PositiveInfinity })
            Assert.Throws<ArgumentOutOfRangeException>(() => DispatcherService.Dispatch(Create(), distance));
        Assert.Throws<ArgumentOutOfRangeException>(() => DispatcherService.Dispatch(Create(price: -1m), 0));
        foreach (double weight in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity })
            Assert.Throws<ArgumentOutOfRangeException>(() => DispatcherService.Dispatch(Create(weight: weight), 0));
    }

    [Fact, Trait("Unit", "U6"), Trait("Level", "Core")]
    public void U6_ExpressValue_HandlesCombinedFlags_AndEmptyCollection()
    {
        Assert.Equal(0m, Array.Empty<Package>().TotalExpressValue());
        Package[] packages = [Create("A", PackageFlags.Express, 15m),
            Create("B", PackageFlags.Express | PackageFlags.Fragile, 25m), Create("C", price: 10m)];
        Assert.Equal(40m, packages.TotalExpressValue());
    }
}
