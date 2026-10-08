using System.Collections;

namespace PlaylistLab.Core.Collections;

/// <summary>Obojsmerný zoznam s kapacitou, iterovaním a udalosťami. Nie je vláknovo bezpečný.</summary>
/// <typeparam name="T">Typ položiek; zoznam povoľuje opakovanie aj null položky.</typeparam>
public sealed class DoublyLinkedList<T> : IEnumerable<T>
{
    private DoublyLinkedNode<T>? _first;
    private DoublyLinkedNode<T>? _last;
    private int _version;

    /// <summary>Vytvorí prázdny zoznam.</summary>
    /// <param name="capacity">Najvyšší povolený počet položiek.</param>
    /// <exception cref="ArgumentOutOfRangeException">Kapacita nie je kladná.</exception>
    public DoublyLinkedList(int capacity = 20)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Kapacita musí byť kladná.");
        Capacity = capacity;
    }

    /// <summary>Najvyšší povolený počet položiek.</summary>
    public int Capacity { get; }
    /// <summary>Aktuálny počet položiek.</summary>
    public int Count { get; private set; }
    /// <summary>Oznámenie po každom úspešnom pridaní alebo odstránení.</summary>
    public event EventHandler<CollectionChangedEventArgs<T>>? Changed;

    /// <summary>Vráti položku na indexe. Prístup má zložitosť O(n).</summary>
    /// <param name="index">Index od nuly.</param>
    /// <returns>Položka na danom indexe.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index je mimo zoznamu.</exception>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            var node = _first;
            for (int i = 0; i < index; i++) node = node?.Next;
            return (node ?? throw new InvalidOperationException("Poškodené prepojenie uzlov.")).Value;
        }
    }

    /// <summary>Pridá položku na začiatok. Pripravená operácia na porovnanie s AddLast.</summary>
    /// <param name="item">Položka, ktorú pridáme.</param>
    /// <exception cref="InvalidOperationException">Zoznam je plný.</exception>
    public void AddFirst(T item)
    {
        EnsureSpace();
        var node = new DoublyLinkedNode<T>(item) { Next = _first };
        if (_first is null) _last = node;
        else _first.Previous = node;
        _first = node;
        Count++;
        _version++;
        OnChanged(CollectionChangeKind.Added, item);
    }

    /// <summary>Pridá položku na koniec v O(1).</summary>
    /// <param name="item">Položka, ktorú pridáme.</param>
    /// <exception cref="InvalidOperationException">Zoznam je plný; obsah sa nemení.</exception>
    public void AddLast(T item)
    {
        // BEGIN U2a
        // TODO U2a: Pridajte položku na koniec podľa kontraktu.
        throw new NotImplementedException("U2a: Pridajte položku na koniec podľa kontraktu.");
        // END U2a
    }

    /// <summary>Odstráni prvú položku v O(1).</summary>
    /// <returns>Odstránená položka.</returns>
    /// <exception cref="InvalidOperationException">Zoznam je prázdny.</exception>
    public T RemoveFirst()
    {
        // BEGIN U2b
        // TODO U2b: Odstráňte prvú položku a opravte krajné uzly.
        throw new NotImplementedException("U2b: Odstráňte prvú položku a opravte krajné uzly.");
        // END U2b
    }

    /// <summary>Odstráni poslednú položku. Pripravená operácia na porovnanie s RemoveFirst.</summary>
    /// <returns>Odstránená položka.</returns>
    /// <exception cref="InvalidOperationException">Zoznam je prázdny.</exception>
    public T RemoveLast()
    {
        var node = _last ?? throw new InvalidOperationException("Zoznam je prázdny.");
        _last = node.Previous;
        if (_last is null) _first = null;
        else _last.Next = null;
        node.Previous = null;
        Count--;
        _version++;
        OnChanged(CollectionChangeKind.Removed, node.Value);
        return node.Value;
    }

    /// <summary>Vytvorí nový dopredný enumerátor. Zmena zoznamu ho zneplatní.</summary>
    /// <returns>Samostatný enumerátor.</returns>
    public IEnumerator<T> GetEnumerator() => EnumerateForward(_version).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Poskytne spätné iterovanie bez vytvorenia kópie.</summary>
    /// <returns>Postupnosť od poslednej položky po prvú.</returns>
    /// <exception cref="InvalidOperationException">Zoznam sa po získaní postupnosti zmení.</exception>
    public IEnumerable<T> Reverse() => EnumerateReverse(_version);

    private IEnumerable<T> EnumerateForward(int expectedVersion)
    {
        // BEGIN U3a
        // TODO U3a: Iterujte od _first cez Next pomocou yield.
        throw new NotImplementedException("U3a: Iterujte od _first cez Next pomocou yield.");
        // END U3a
    }

    private IEnumerable<T> EnumerateReverse(int expectedVersion)
    {
        // BEGIN U3b
        // TODO U3b: Iterujte od _last cez Previous pomocou yield.
        throw new NotImplementedException("U3b: Iterujte od _last cez Previous pomocou yield.");
        // END U3b
    }

    private void OnChanged(CollectionChangeKind kind, T item)
    {
        // BEGIN U4
        // TODO U4: Oznámte úspešnú zmenu cez Changed?.Invoke.
        // Prázdne telo zatiaľ umožní skúšať iné bloky nezávisle.
        _ = Changed;
        // END U4
    }

    /// <summary>Vytvorí lenivo vyhodnocovaný filter. Samotné volanie nespúšťa predikát.</summary>
    /// <param name="predicate">Podmienka prijatia položky.</param>
    /// <returns>Vyhovujúce položky v pôvodnom poradí vrátane opakovaní.</returns>
    /// <exception cref="ArgumentNullException">Predikát je null.</exception>
    /// <exception cref="InvalidOperationException">Zoznam sa počas prechádzania zmení.</exception>
    public IEnumerable<T> WhereMatches(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return FilterIterator(predicate);
    }

    private IEnumerable<T> FilterIterator(Func<T, bool> predicate)
    {
        // BEGIN U5
        // TODO U5: Vráťte vyhovujúce položky cez yield.
        throw new NotImplementedException("U5: Vráťte vyhovujúce položky cez yield.");
        // END U5
    }

    private void EnsureSpace()
    {
        if (Count >= Capacity)
            throw new InvalidOperationException($"Kapacita {Capacity} je vyčerpaná.");
    }

    private void EnsureUnchanged(int expectedVersion)
    {
        if (expectedVersion != _version)
            throw new InvalidOperationException("Zoznam sa počas iterovania zmenil.");
    }
}
