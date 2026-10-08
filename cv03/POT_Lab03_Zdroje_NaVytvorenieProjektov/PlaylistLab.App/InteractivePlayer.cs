using PlaylistLab.Core.Collections;
using PlaylistLab.Core.Models;

namespace PlaylistLab.App;

// Pripravené jednoduché menu. Stav sa uchováva iba počas jedného spustenia.
internal static class InteractivePlayer
{
    internal static void Run()
    {
        try
        {
            var catalog = SampleData.CreateCatalog();
            var playlist = new DoublyLinkedList<Track>(3);
            playlist.Changed += (_, e) => Console.WriteLine($"Zmena {e.Kind}: {e.Item.Id}, počet {e.Count}");
            Console.WriteLine("Playlist Lab. Kapacita 3. Príkazy: catalog, add ID, list, next, reverse, filter SEKUNDY, exit");
            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (input is null || input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase)) return;
                var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;
                try
                {
                    switch (parts[0].ToLowerInvariant())
                    {
                        case "catalog": PlaylistView.Print(catalog.Items); break;
                        case "list": PlaylistView.Print(playlist); break;
                        case "reverse": PlaylistView.Print(playlist.Reverse()); break;
                        case "next": Console.WriteLine($"Odobraté: {playlist.RemoveFirst().Title}"); break;
                        case "add" when parts.Length == 2:
                            if (catalog.TryGet(parts[1], out var track)) playlist.AddLast(track);
                            else Console.WriteLine("Toto ID nie je v registri.");
                            break;
                        case "filter" when parts.Length == 2:
                            if (int.TryParse(parts[1], out int limit) && limit > 0)
                                PlaylistView.Print(playlist.WhereMatches(t => t.DurationSeconds <= limit));
                            else Console.WriteLine("Zadajte kladný počet sekúnd.");
                            break;
                        default: Console.WriteLine("Príkazy: catalog, add ID, list, next, reverse, filter SEKUNDY, exit"); break;
                    }
                }
                catch (InvalidOperationException ex) { Console.WriteLine(ex.Message); }
                catch (ArgumentException ex) { Console.WriteLine(ex.Message); }
                catch (NotImplementedException ex) { Console.WriteLine($"Táto operácia ešte čaká na doplnenie: {ex.Message}"); }
            }
        }
        catch (NotImplementedException ex)
        {
            Console.WriteLine($"Najprv dokončite U2 až U5: {ex.Message}");
        }
    }
}
