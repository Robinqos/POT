using System.Diagnostics.CodeAnalysis;
using PlaylistLab.Core.Models;

namespace PlaylistLab.Core.Services;

/// <summary>Register jedinečných ID bez rozlišovania veľkosti písmen.</summary>
/// <typeparam name="T">Referenčný typ s vlastnosťou Id.</typeparam>
public sealed class ItemCatalog<T> where T : class, IIdentifiable
{
    private readonly Dictionary<string, T> _items = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Počet jedinečných položiek.</summary>
    public int Count => _items.Count;
    /// <summary>Pohľad na hodnoty registra. Poradie nie je súčasťou kontraktu.</summary>
    public IEnumerable<T> Items => _items.Values;

    /// <summary>Zaregistruje novú položku bez prepísania existujúceho ID.</summary>
    /// <param name="item">Položka s neprázdnym ID.</param>
    /// <returns>True pri vložení, false pri duplicitnom ID.</returns>
    /// <exception cref="ArgumentNullException">Položka je null.</exception>
    /// <exception cref="ArgumentException">ID je prázdne alebo obsahuje len medzery.</exception>
    public bool Register(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Id);
        return _items.TryAdd(item.Id, item);
    }

    /// <summary>Pokúsi sa nájsť položku podľa ID.</summary>
    /// <param name="id">Neprázdne ID.</param>
    /// <param name="item">Nájdená položka pri true, inak null.</param>
    /// <returns>True, ak ID existuje.</returns>
    /// <exception cref="ArgumentException">ID je prázdne.</exception>
    /// <exception cref="ArgumentNullException">ID je null.</exception>
    public bool TryGet(string id, [NotNullWhen(true)] out T? item)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _items.TryGetValue(id, out item);
    }

    /// <summary>Vráti položku alebo null pri neexistujúcom ID.</summary>
    /// <param name="id">Neprázdne ID.</param>
    /// <returns>Nájdená položka alebo null.</returns>
    /// <exception cref="ArgumentException">ID je prázdne.</exception>
    /// <exception cref="ArgumentNullException">ID je null.</exception>
    public T? GetOrDefault(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _items.GetValueOrDefault(id);
    }
}
