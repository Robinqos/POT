using PlaylistLab.Core.Models;

namespace PlaylistLab.App;

internal static class PlaylistView
{
    // U6 NuGet: po pridaní Spectre.Console nahraďte tento súbor vzorom z návodu.
    internal static bool UsesSpectre => false;

    internal static void Print(IEnumerable<Track> tracks)
    {
        foreach (var track in tracks)
            Console.WriteLine($"{track.Id}: {track.Title}, {track.Artist}, {track.Album ?? "Bez albumu"}, {track.DurationSeconds} s");
    }
}
