# PlaylistLab.Core

Didaktická knižnica pre POT Lab03, .NET 10.

Obsahuje obojsmerný zoznam s kapacitou, dopredné a spätné iterovanie,
udalosti po zmene a generický register položiek podľa ID.
Kolekcie sú určené na používanie z jedného vlákna.

Príklad:

```csharp
var list = new PlaylistLab.Core.Collections.DoublyLinkedList<int>(3);
list.AddLast(10);
list.AddLast(20);
Console.WriteLine(string.Join(", ", list.Reverse()));
```
