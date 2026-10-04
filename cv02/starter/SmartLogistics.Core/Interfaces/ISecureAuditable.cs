namespace SmartLogistics.Core.Interfaces;

/// <summary>
/// Rozhranie pre interný audit a bezpečnosť.
/// Demonštruje explicitnú implementáciu rozhrania, ktorá slúži na „skrytie“
/// interných/citlivých metód pred bežným verejným rozhraním triedy. Nejde o autorizáciu; každý držiteľ objektu ho môže pretypovať.
/// </summary>
public interface ISecureAuditable
{
    string GetAuditRecord();
}
