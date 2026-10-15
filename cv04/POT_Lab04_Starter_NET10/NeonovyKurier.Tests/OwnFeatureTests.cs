using System.Net;
using NeonovyKurier.Core;

namespace NeonovyKurier.Tests;

// U6: sem pridajte aspoň tri vlastné testy DownloadWithCacheAsync. Vzory sú v DownloadTests.cs.
//
// ControlledHandler nahrádza sieť:
// - požiadavku identifikuje cesta bez lomky: Route "/theme" -> "theme",
// - handler.Complete("theme", "obsah") alebo Complete("theme", "chyba", HttpStatusCode.ServiceUnavailable)
//   pripraví odpoveď; môžete ho zavolať pred aj po začatí požiadavky,
// - await handler.StartedAsync("theme") počká, kým služba požiadavku naozaj začne.
public sealed class OwnFeatureTests
{
    private static readonly GameAsset Theme = new("theme", "theme.json", "/theme", Required: false);

    private static HttpClient Client(ControlledHandler handler)
        => new(handler) { BaseAddress = new Uri("http://localhost/") };

    // Dočasný priečinok pre cache. Na konci testu ho zmažte vo finally.
    private static string NewCacheDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), "NeonovyKurierTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    // Kostra prvého testu. Odkomentujte ju a doplňte časti označené "...".
    //
    // [Fact]
    // public async Task U6_HttpFailure_OptionalAssetWithCache_ReturnsCachedContent()
    // {
    //     string directory = NewCacheDirectory();
    //     try
    //     {
    //         await File.WriteAllTextAsync(Path.Combine(directory, Theme.FileName), "OLD");
    //         using var handler = new ControlledHandler();
    //         using var client = Client(handler);
    //         handler.Complete("theme", "Výpadok", HttpStatusCode.ServiceUnavailable);
    //
    //         DownloadResult result = await new DownloadService(client).DownloadWithCacheAsync(Theme, directory);
    //
    //         Assert.Equal(...);
    //     }
    //     finally { Directory.Delete(directory, true); }
    // }
}
