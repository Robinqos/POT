using SmartLogistics.Core.Interfaces;

namespace SmartLogistics.Core.Models;

/// <summary>
/// U4: Implicitná vs. Explicitná implementácia rozhraní.
/// 
/// Využite GitHub Copilot:
/// Prompt do Copilot Chatu:
/// "Implementuj rozhranie ISecureAuditable v triede TrackedPackage EXPLICITNE, aby metóda GetAuditRecord nebola viditeľná vo verejnom API inštancie, ale len cez pretypovanie."
/// </summary>
public class TrackedPackage : ITrackable, ISecureAuditable
{
    public Package Package { get; }
    public string CurrentLocation { get; private set; }
    private readonly List<string> _auditTrail = [];

    public TrackedPackage(Package package, string initialLocation)
    {
        Package = package;
        CurrentLocation = initialLocation;
        _auditTrail.Add($"Vytvorené v lokalite {initialLocation} - {DateTime.UtcNow:s}");
    }

    // 1. Implicitná implementácia ITrackable: Verejná metóda
    public string GetStatus() =>
        $"Balík {Package.TrackingNumber} sa aktuálne nachádza: {CurrentLocation}.";

    // TODO: U4 - Implementujte rozhranie ISecureAuditable EXPLICITNE.
    // POZOR: Metóda NESMIE mať modifikátor public a jej názov musí byť 'ISecureAuditable.GetAuditRecord()'.
    string ISecureAuditable.GetAuditRecord()
    {
        throw new NotImplementedException("U4: Implementujte explicitné rozhranie ISecureAuditable.GetAuditRecord().");
    }

    public void UpdateLocation(string newLocation)
    {
        CurrentLocation = newLocation;
        _auditTrail.Add($"Presun do {newLocation} - {DateTime.UtcNow:s}");
    }
}
