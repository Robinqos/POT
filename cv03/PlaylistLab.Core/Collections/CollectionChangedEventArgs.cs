namespace PlaylistLab.Core.Collections;

/// <summary>Druh úspešnej zmeny kolekcie.</summary>
public enum CollectionChangeKind
{
    /// <summary>Položka bola pridaná.</summary>
    Added,
    /// <summary>Položka bola odstránená.</summary>
    Removed
}

/// <summary>Údaje o zmene kolekcie po dokončení operácie.</summary>
/// <typeparam name="T">Typ položky.</typeparam>
/// <param name="kind">Druh zmeny.</param>
/// <param name="item">Pridaná alebo odstránená položka.</param>
/// <param name="count">Počet položiek po zmene.</param>
public sealed class CollectionChangedEventArgs<T>(CollectionChangeKind kind, T item, int count) : EventArgs
{
    /// <summary>Druh zmeny.</summary>
    public CollectionChangeKind Kind { get; } = kind;
    /// <summary>Položka, ktorej sa zmena týka.</summary>
    public T Item { get; } = item;
    /// <summary>Počet položiek po zmene.</summary>
    public int Count { get; } = count;
}
