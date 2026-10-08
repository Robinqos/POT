using PlaylistLab.Core.Collections;
using PlaylistLab.Core.Models;
using PlaylistLab.Core.Services;

namespace PlaylistLab.Tests;

[Trait("Unit", "U1")]
public class PreparedTests
{
    [Fact]
    public void U1_TrackStoresOptionalAlbum()
    {
        var track = new Track("T01", "Skladba", "Interpret", 180);
        Assert.Null(track.Album);
        Assert.Equal(180, track.DurationSeconds);
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(int.MinValue)]
    public void U1_InvalidDurationIsRejected(int seconds) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Track("T01", "Skladba", "Interpret", seconds));

    [Theory]
    [InlineData(0)] [InlineData(-1)]
    public void U1_InvalidCapacityIsRejected(int capacity) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new DoublyLinkedList<int>(capacity));

    [Theory]
    [InlineData(-1)] [InlineData(0)]
    public void U1_InvalidIndexIsRejected(int index) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new DoublyLinkedList<int>()[index]);

    [Fact]
    public void U1_PreparedEndsAndIndexerWork()
    {
        var list = new DoublyLinkedList<int>();
        list.AddFirst(20); list.AddFirst(10);
        Assert.Equal(10, list[0]); Assert.Equal(20, list[1]);
        Assert.Equal(20, list.RemoveLast()); Assert.Equal(10, list.RemoveLast());
        Assert.Equal(0, list.Count);
        list.AddFirst(30);
        Assert.Equal(30, list[0]);
    }

    [Fact]
    public void U1_EmptyRemoveLastIsRejected() =>
        Assert.Throws<InvalidOperationException>(() => new DoublyLinkedList<int>().RemoveLast());

    [Fact]
    public void U1_GenericListCanContainNull()
    {
        var list = new DoublyLinkedList<string?>();
        list.AddFirst(null);
        Assert.Null(list[0]); Assert.Null(list.RemoveLast());
    }
}

[Trait("Unit", "U2")]
public class EndOperationTests
{
    [Fact]
    public void U2_AddLastPreservesInsertionOrder()
    {
        var list = new DoublyLinkedList<int>();
        list.AddLast(10); list.AddLast(20); list.AddLast(10);
        Assert.Equal(3, list.Count);
        Assert.Equal(10, list[0]); Assert.Equal(20, list[1]); Assert.Equal(10, list[2]);
    }

    [Fact]
    public void U2_AddLastAfterAddFirstRepairsBothEnds()
    {
        var list = new DoublyLinkedList<int>();
        list.AddFirst(20); list.AddFirst(10); list.AddLast(30);
        Assert.Equal(30, list.RemoveLast()); Assert.Equal(20, list.RemoveLast());
        Assert.Equal(10, list.RemoveLast()); Assert.Equal(0, list.Count);
    }

    [Fact]
    public void U2_FullListRejectsAddWithoutMutation()
    {
        var list = new DoublyLinkedList<int>(2);
        list.AddLast(10); list.AddLast(20);
        Assert.Throws<InvalidOperationException>(() => list.AddLast(30));
        Assert.Equal(2, list.Count); Assert.Equal(10, list[0]); Assert.Equal(20, list[1]);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(5)]
    public void U2_RemoveFirstPreservesRemainingOrder(int count)
    {
        var list = new DoublyLinkedList<int>();
        for (int n = count; n >= 1; n--) list.AddFirst(n);
        for (int n = 1; n <= count; n++)
        {
            Assert.Equal(n, list.RemoveFirst());
            Assert.Equal(count - n, list.Count);
            if (list.Count > 0) Assert.Equal(n + 1, list[0]);
        }
        list.AddFirst(99); Assert.Equal(99, list.RemoveLast());
    }

    [Fact]
    public void U2_EmptyRemoveFirstIsRejected() =>
        Assert.Throws<InvalidOperationException>(() => new DoublyLinkedList<int>().RemoveFirst());

    [Fact]
    public void U2_MixedOperationsKeepLinksConsistent()
    {
        var list = new DoublyLinkedList<string>(3);
        list.AddLast("A"); Assert.Equal("A", list.RemoveFirst());
        list.AddLast("B"); list.AddFirst("A"); list.AddLast("C");
        Assert.Equal("C", list.RemoveLast()); Assert.Equal("A", list.RemoveFirst());
        Assert.Equal("B", list.RemoveLast()); Assert.Equal(0, list.Count);
    }
}

[Trait("Unit", "U3")]
public class IteratorTests
{
    [Fact]
    public void U3_EmptyForwardHasNoItems() => Assert.Empty(new DoublyLinkedList<int>());
    [Fact]
    public void U3_ForwardKeepsOrderAndDuplicates() => Assert.Equal(new[] { 1, 2, 1 }, Make(1, 2, 1).ToArray());
    [Fact]
    public void U3_EmptyReverseHasNoItems() => Assert.Empty(new DoublyLinkedList<int>().Reverse());
    [Fact]
    public void U3_ReverseUsesPreviousLinks() => Assert.Equal(new[] { 3, 2, 1 }, Make(1, 2, 3).Reverse().ToArray());

    [Fact]
    public void U3_EnumeratorsHaveIndependentPositions()
    {
        var list = Make(1, 2, 3);
        using var a = list.GetEnumerator(); using var b = list.GetEnumerator();
        Assert.True(a.MoveNext()); Assert.True(a.MoveNext()); Assert.Equal(2, a.Current);
        Assert.True(b.MoveNext()); Assert.Equal(1, b.Current);
    }

    [Fact]
    public void U3_MutationDuringEnumerationIsRejected()
    {
        var list = Make(1, 2, 3);
        using var e = list.GetEnumerator(); Assert.True(e.MoveNext());
        list.RemoveLast();
        Assert.Throws<InvalidOperationException>(() => e.MoveNext());
    }

    [Fact]
    public void U3_MutationBeforeFirstMoveNextIsRejected()
    {
        var list = Make(1, 2);
        using var e = list.GetEnumerator(); list.AddFirst(0);
        Assert.Throws<InvalidOperationException>(() => e.MoveNext());
    }

    [Fact]
    public void U3_ReverseSequenceIsInvalidatedByMutation()
    {
        var list = Make(1, 2, 3); var reverse = list.Reverse(); list.RemoveLast();
        Assert.Throws<InvalidOperationException>(() => reverse.ToArray());
    }

    internal static DoublyLinkedList<int> Make(params int[] values)
    {
        var list = new DoublyLinkedList<int>();
        for (int i = values.Length - 1; i >= 0; i--) list.AddFirst(values[i]);
        return list;
    }
}

[Trait("Unit", "U4")]
public class EventTests
{
    [Fact]
    public void U4_AddEventDescribesCompletedState()
    {
        var list = new DoublyLinkedList<int>(); int calls = 0;
        list.Changed += (sender, e) =>
        {
            calls++; Assert.Same(list, sender); Assert.Equal(CollectionChangeKind.Added, e.Kind);
            Assert.Equal(10, e.Item); Assert.Equal(1, e.Count); Assert.Equal(1, list.Count);
        };
        list.AddFirst(10); Assert.Equal(1, calls);
    }

    [Fact]
    public void U4_RemoveEventUsesCountAfterRemoval()
    {
        var list = new DoublyLinkedList<int>(); list.AddFirst(10); int calls = 0;
        list.Changed += (_, e) =>
        {
            calls++; Assert.Equal(CollectionChangeKind.Removed, e.Kind);
            Assert.Equal(10, e.Item); Assert.Equal(0, e.Count); Assert.Equal(0, list.Count);
        };
        list.RemoveLast(); Assert.Equal(1, calls);
    }

    [Fact]
    public void U4_UnsubscribeStopsOnlyThatSubscriber()
    {
        var list = new DoublyLinkedList<int>(); int a = 0, b = 0;
        EventHandler<CollectionChangedEventArgs<int>> first = (_, _) => a++;
        EventHandler<CollectionChangedEventArgs<int>> second = (_, _) => b++;
        list.Changed += first; list.Changed += second;
        list.AddFirst(1); list.Changed -= second; list.AddFirst(2);
        Assert.Equal(2, a); Assert.Equal(1, b);
    }

    [Fact]
    public void U4_RejectedAddDoesNotNotify()
    {
        var list = new DoublyLinkedList<int>(1); list.AddFirst(1); int calls = 0;
        list.Changed += (_, _) => calls++;
        Assert.Throws<InvalidOperationException>(() => list.AddFirst(2)); Assert.Equal(0, calls);
    }

    [Fact]
    public void U4_RejectedRemovalDoesNotNotify()
    {
        var list = new DoublyLinkedList<int>(); int calls = 0;
        list.Changed += (_, _) => calls++;
        Assert.Throws<InvalidOperationException>(() => list.RemoveLast()); Assert.Equal(0, calls);
    }

    [Fact]
    public void U4_StudentEndOperationsNotifyExactlyOnce()
    {
        var list = new DoublyLinkedList<int>(); var kinds = new List<CollectionChangeKind>();
        list.Changed += (_, e) => kinds.Add(e.Kind);
        list.AddLast(10); list.RemoveFirst();
        Assert.Equal(new[] { CollectionChangeKind.Added, CollectionChangeKind.Removed }, kinds);
    }
}

[Trait("Unit", "U1")]
public class CatalogTests
{
    [Fact]
    public void U1_CatalogFindsIdWithoutCaseSensitivity()
    {
        var catalog = new ItemCatalog<Track>(); var track = new Track("T01", "Skladba", "Interpret", 180);
        Assert.True(catalog.Register(track)); Assert.True(catalog.TryGet("t01", out var found));
        Assert.Same(track, found); Assert.Equal(1, catalog.Count);
    }

    [Fact]
    public void U1_DuplicateDoesNotReplaceOriginal()
    {
        var catalog = new ItemCatalog<Track>(); var original = new Track("T01", "Pôvodná", "Interpret", 180);
        catalog.Register(original);
        Assert.False(catalog.Register(new Track("t01", "Nová", "Interpret", 60)));
        Assert.Equal(1, catalog.Count); Assert.Same(original, catalog.GetOrDefault("t01"));
    }

    [Fact]
    public void U1_UnknownIdReturnsFalseAndNull()
    {
        var catalog = new ItemCatalog<Track>(); Assert.False(catalog.TryGet("missing", out var item)); Assert.Null(item);
    }

    [Fact]
    public void U1_NullItemIsRejected() =>
        Assert.Throws<ArgumentNullException>(() => new ItemCatalog<Track>().Register(null!));

    [Theory]
    [InlineData("")] [InlineData("  ")]
    public void U1_InvalidIdIsRejected(string id) =>
        Assert.Throws<ArgumentException>(() => new ItemCatalog<OtherItem>().Register(new OtherItem(id)));

    [Fact]
    public void U1_CatalogWorksWithSecondReferenceType()
    {
        var catalog = new ItemCatalog<OtherItem>(); var item = new OtherItem("P01");
        Assert.True(catalog.Register(item)); Assert.True(catalog.TryGet("p01", out var result)); Assert.Same(item, result);
    }

    [Fact]
    public void U1_PreparedNullableLookupReturnsNull() => Assert.Null(new ItemCatalog<Track>().GetOrDefault("missing"));

    [Fact]
    public void U1_TryGetRejectsNullKey() =>
        Assert.Throws<ArgumentNullException>(() => new ItemCatalog<Track>().TryGet(null!, out _));

    private sealed record OtherItem(string Id) : IIdentifiable;
}

[Trait("Unit", "U5")]
public class FilterTests
{
    [Fact]
    public void U5_EmptyFilterHasNoItems() => Assert.Empty(new DoublyLinkedList<int>().WhereMatches(x => x > 0));

    [Fact]
    public void U5_FilterPreservesOrderAndDuplicates() =>
        Assert.Equal(new[] { 2, 2, 4 }, IteratorTests.Make(1, 2, 2, 3, 4).WhereMatches(x => x % 2 == 0).ToArray());

    [Fact]
    public void U5_PredicateRunsOnlyDuringEnumeration()
    {
        var list = IteratorTests.Make(1, 2, 3); int calls = 0;
        var query = list.WhereMatches(x => { calls++; return x > 1; });
        Assert.Equal(0, calls); Assert.Equal(new[] { 2, 3 }, query.ToArray()); Assert.Equal(3, calls);
    }

    [Fact]
    public void U5_NullPredicateIsRejectedImmediately() =>
        Assert.Throws<ArgumentNullException>(() => new DoublyLinkedList<int>().WhereMatches(null!));

    [Fact]
    public void U5_QuerySeesChangesBeforeEnumerationStarts()
    {
        var list = IteratorTests.Make(1, 2); var query = list.WhereMatches(x => x > 1); list.AddFirst(3);
        Assert.Equal(new[] { 3, 2 }, query.ToArray());
    }

    [Fact]
    public void U5_ClosureChangesQueryButNotMaterializedSnapshot()
    {
        var list = IteratorTests.Make(1, 2, 3); int limit = 2;
        var query = list.WhereMatches(x => x <= limit); var snapshot = query.ToList(); limit = 1;
        Assert.Equal(new[] { 1 }, query.ToArray()); Assert.Equal(new[] { 1, 2 }, snapshot);
    }

    [Fact]
    public void U5_CatalogAndPlaylistAllowDifferentDuplicateRules()
    {
        var catalog = new ItemCatalog<Track>(); var track = new Track("T01", "Skladba", "Interpret", 180);
        catalog.Register(track); var list = new DoublyLinkedList<Track>(3);
        if (!catalog.TryGet("t01", out var found)) throw new InvalidOperationException("Chýba zaregistrovaná skladba.");
        list.AddLast(found); list.AddLast(found);
        Assert.Equal(1, catalog.Count); Assert.Equal(2, list.Count);
        Assert.Equal(new[] { "T01", "T01" }, list.WhereMatches(t => t.DurationSeconds <= 180).Select(t => t.Id).ToArray());
    }
}
