using PlaylistLab.Core.Models;
using PlaylistLab.Core.Services;

namespace PlaylistLab.App;

// Vymyslené názvy a interpreti, všetky dĺžky sú v sekundách.
internal static class SampleData
{
    internal static Track[] Tracks =>
    [
        new("T01", "Ranný spoj", "POT Band", 180),
        new("T02", "Nočný vlak", "POT Band", 240, "Cesty"),
        new("T03", "Krátka pauza", "Campus Trio", 90),
        new("T04", "Dlhá cesta", "Campus Trio", 360, "Na cestách")
    ];

    internal static ItemCatalog<Track> CreateCatalog()
    {
        var catalog = new ItemCatalog<Track>();
        foreach (var track in Tracks) catalog.Register(track);
        return catalog;
    }
}
