namespace SmartLogistics.Core.Security;

/// <summary>
/// Trieda demonštrujúca všetkých 6 prístupových modifikátorov C# v rámci zostavenia SmartLogistics.Core.
/// Údaje sú fiktívne; tento príklad nerealizuje autentifikáciu ani autorizáciu.
/// </summary>
public class FleetSecurityBase
{
    // 1. private: prístupné len vo vnútri tejto triedy
    private string MasterKey { get; set; } = "DEMO-KEY";

    // 2. public: prístupné odkiaľkoľvek
    public string PublicTelemetryUrl { get; set; } = "https://telemetry.example.invalid";

    // 3. protected: prístupné v tejto triede a vo VŠETKÝCH odvodených triedach (aj v inom projekte)
    protected string VehiclePinCode { get; set; } = "1234";

    // 4. internal: prístupné ZO VŠETKÝCH tried v tomto zostavení (SmartLogistics.Core),
    //    ale NEPRÍSTUPNÉ v inom projekte (SmartLogistics.App)
    internal string DepotEncryptionKey { get; set; } = "DEMO-DEPOT-KEY";

    // 5. protected internal: prístupné zo všetkých tried v tomto projekte
    //    ALEBO z odvodených tried v iných projektoch
    protected internal string MaintenanceChannel { get; set; } = "CH-RADIO-07";

    // 6. private protected: prístupné LEN z odvodených tried v tomto TOM ISTOM projekte
    private protected string HardwareSerialPrefix { get; set; } = "HW-CORE-";

    public string TestInternalAccess() =>
        $"Demonštračné hodnoty: {MasterKey}, {InternalPropCheck()}";

    private string InternalPropCheck() => DepotEncryptionKey;
}

/// <summary>
/// Odvodená trieda v tom istom projekte.
/// Má prístup k: public, protected, internal, protected internal, private protected.
/// NEMÁ prístup len k private.
/// </summary>
public class LocalFleetSubSecurity : FleetSecurityBase
{
    public string DescribeAccess()
    {
        return $"Protected: {VehiclePinCode}, Internal: {DepotEncryptionKey}, " +
               $"ProtInternal: {MaintenanceChannel}, PrivProtected: {HardwareSerialPrefix}";
    }
}

/// <summary>
/// Trieda s modifikátorom 'internal' – je viditeľná IBA v SmartLogistics.Core.
/// V SmartLogistics.App sa vôbec nedá použiť (spôsobí CS0122).
/// </summary>
internal class InternalStationDiagnostic
{
    public static string RunSelfTest() => "Všetky systémy depa sú zelené.";
}
