namespace SmartLogistics.Core.Legacy;

/// <summary>
/// LEGACY KÓD: Demonštruje starý štýl písania tried z C# 4 / .NET Framework 4.5.
/// Obsahuje manuálne zapuzdrené privátne fields, zdĺhavé get/set metódy,
/// chýbajúce immutability, absenciu hodnotového porovnávania a starý konštruktor.
/// Úlohou študenta je pomocou GitHub Copilot Chatu zrefaktorovať tento kód
/// do moderného 'record class' s primárnym konštruktorom a init-only vlastnosťami.
/// </summary>
public class LegacyPackage
{
    private string _trackingNumber;
    private decimal _basePrice;
    private double _weightKg;

    public string TrackingNumber
    {
        get { return _trackingNumber; }
        set { _trackingNumber = value; }
    }

    public decimal BasePrice
    {
        get { return _basePrice; }
        set { _basePrice = value; }
    }

    public double WeightKg
    {
        get { return _weightKg; }
        set { _weightKg = value; }
    }

    public LegacyPackage(string trackingNumber, decimal basePrice, double weightKg)
    {
        this._trackingNumber = trackingNumber;
        this._basePrice = basePrice;
        this._weightKg = weightKg;
    }
}
