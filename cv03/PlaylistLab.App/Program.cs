using PlaylistLab.App;
using PlaylistLab.Core.Collections;
using PlaylistLab.Core.Models;
using PlaylistLab.Core.Services;

if (args.Contains("--interactive", StringComparer.OrdinalIgnoreCase))
{
    InteractivePlayer.Run();
    return;
}

bool basicOnly = args.Length == 1 && args[0] == "--basic";
string? selected = null;
if (args.Length == 2 && args[0] == "--unit") selected = args[1].ToUpperInvariant();
else if (args.Length != 0 && !basicOnly)
{
    Console.Error.WriteLine("Použitie: --basic, --unit U1 až U6 alebo --interactive");
    Environment.ExitCode = 2;
    return;
}
if (selected is not null && !new[] { "U1", "U2", "U3", "U4", "U5", "U6" }.Contains(selected))
{
    Console.Error.WriteLine("Neznámy blok. Použite U1 až U6.");
    Environment.ExitCode = 2;
    return;
}

int pending = 0, errors = 0;
Run("U1", () =>
{
    var values = new DoublyLinkedList<int>(3);
    values.AddFirst(20);
    values.AddFirst(10);
    Console.WriteLine($"Pripravené operácie: {values[0]}, {values[1]}; od konca {values.RemoveLast()}");
    Console.WriteLine($"Album T01: {SampleData.Tracks[0].Album ?? "Bez albumu"}");
    return values.Count == 1 && values[0] == 10;
});
Run("U1", () =>
{
    var catalog = SampleData.CreateCatalog();
    bool duplicate = catalog.Register(new Track("t01", "Iný názov", "Iný interpret", 60));
    bool found;
    Console.WriteLine($"Register {catalog.Count}, duplicitné vloženie {duplicate}");
    if (catalog.TryGet("t01", out var track))
    {
        found = true;
        Console.WriteLine($"Nájdené: {track.Title}; album {track.Album ?? "Bez albumu"}");
    }
    else found = false;
    Console.WriteLine(catalog.GetOrDefault("T99")?.Title ?? "Nenájdené");
    return catalog.Count == 4 && !duplicate && found && track?.Title == "Ranný spoj";
});
Run("U2", () =>
{
    var values = new DoublyLinkedList<int>(3);
    values.AddLast(10); values.AddLast(20); values.AddLast(30);
    bool rejected = false;
    try { values.AddLast(40); }
    catch (InvalidOperationException ex) { rejected = true; Console.WriteLine(ex.Message); }
    int first = values.RemoveFirst();
    Console.WriteLine($"Odobraté {first}; zostáva {values[0]}, {values[1]}; počet {values.Count}");
    return rejected && first == 10 && values.Count == 2 && values[0] == 20 && values[1] == 30;
});
Run("U3", () =>
{
    var values = PreparedNumbers();
    int[] forward = values.ToArray();
    int[] reverse = values.Reverse().ToArray();
    Console.WriteLine($"Dopredu: {string.Join(", ", forward)}");
    Console.WriteLine($"Dozadu: {string.Join(", ", reverse)}");
    using var e = values.GetEnumerator();
    e.MoveNext(); Console.WriteLine($"MoveNext + Current: {e.Current}");
    return forward.SequenceEqual(new[] { 1, 2, 3 }) && reverse.SequenceEqual(new[] { 3, 2, 1 });
});
Run("U4", () =>
{
    var values = new DoublyLinkedList<int>();
    int notifications = 0;
    var history = new List<string>();
    EventHandler<CollectionChangedEventArgs<int>> counter = (_, _) => notifications++;
    EventHandler<CollectionChangedEventArgs<int>> logger = (_, e) => history.Add($"{e.Kind}: {e.Item}, počet {e.Count}");
    values.Changed += counter;
    values.Changed += logger;
    values.AddFirst(10);
    values.Changed -= logger;
    values.RemoveLast();
    Console.WriteLine($"Počítadlo {notifications}, história {history.Count}; očakávame 2 a 1");
    foreach (var line in history) Console.WriteLine(line);
    return notifications == 2 && history.Count == 1;
});
Run("U5", () =>
{
    var playlist = new DoublyLinkedList<Track>(3);
    var tracks = SampleData.Tracks;
    playlist.AddFirst(tracks[0]); playlist.AddFirst(tracks[1]); playlist.AddFirst(tracks[2]);
    int limit = 180, calls = 0;
    var query = playlist.WhereMatches(t => { calls++; return t.DurationSeconds <= limit; });
    Console.WriteLine($"Po vytvorení filtra: {calls} volaní predikátu");
    var snapshot = query.ToList();
    Console.WriteLine($"Limit 180: {string.Join(", ", snapshot.Select(t => t.Id))}; volaní {calls}");
    limit = 100;
    var second = query.ToList();
    Console.WriteLine($"Limit 100: {string.Join(", ", second.Select(t => t.Id))}; spolu volaní {calls}");
    PlaylistView.Print(snapshot);
    return snapshot.Select(t => t.Id).SequenceEqual(new[] { "T03", "T01" })
        && second.Select(t => t.Id).SequenceEqual(new[] { "T03" }) && calls == 6;
});
Run("U6", () =>
{
    PlaylistView.Print(SampleData.Tracks);
    if (!PlaylistView.UsesSpectre) Console.WriteLine("U6 je dobrovoľná NuGet tabuľka.");
    return PlaylistView.UsesSpectre;
});
Console.WriteLine($"Súhrn: {pending} nedokončených blokov, {errors} neočakávaných chýb.");
Environment.ExitCode = errors == 0 ? 0 : 1;

void Run(string name, Func<bool> demo)
{
    if (basicOnly && name == "U6") return;
    if (selected is not null && selected != name) return;
    Console.WriteLine($"\n=== {name} ===");
    try
    {
        if (demo()) Console.WriteLine("Kontrolný výsledok súhlasí.");
        else { pending++; Console.WriteLine("Kontrolný výsledok zatiaľ nesúhlasí; pokračujte úlohou."); }
    }
    catch (NotImplementedException ex) { pending++; Console.WriteLine($"Čaká na doplnenie: {ex.Message}"); }
    catch (Exception ex) { errors++; Console.Error.WriteLine(ex.ToString()); }
}

static DoublyLinkedList<int> PreparedNumbers()
{
    var values = new DoublyLinkedList<int>();
    values.AddFirst(3); values.AddFirst(2); values.AddFirst(1);
    return values;
}
