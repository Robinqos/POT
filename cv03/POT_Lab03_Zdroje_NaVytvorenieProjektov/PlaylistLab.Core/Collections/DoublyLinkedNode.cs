namespace PlaylistLab.Core.Collections;

// Pripravený vnútorný uzol. Kód aplikácie ho nepotrebuje poznať.
internal sealed class DoublyLinkedNode<T>(T value)
{
    internal T Value { get; } = value;
    internal DoublyLinkedNode<T>? Previous { get; set; }
    internal DoublyLinkedNode<T>? Next { get; set; }
}
