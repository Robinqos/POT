using System.Net;
using NeonovyKurier.Core;

namespace NeonovyKurier.Tests;

public sealed class OwnFeatureTests
{
    private static readonly GameAsset Theme = new("theme", "theme.json", "/theme", Required: false);

    private static HttpClient Client(ControlledHandler handler)
        => new(handler) { BaseAddress = new Uri("http://localhost/") };

    private static string NewCacheDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), "NeonovyKurierTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    // 1. HTTP 503 + voliteľný súbor + existujúca cache → vráti starý obsah, FromCache = true
    [Fact]
    public async Task U6_HttpFailure_OptionalAssetWithCache_ReturnsCachedContent()
    {
        string directory = NewCacheDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(directory, Theme.FileName), "OLD");
            using var handler = new ControlledHandler();
            using var client = Client(handler);
            handler.Complete("theme", "Výpadok", HttpStatusCode.ServiceUnavailable);

            DownloadResult result = await new DownloadService(client)
                .DownloadWithCacheAsync(Theme, directory);

            Assert.Equal("OLD", result.Content);
            Assert.True(result.FromCache);
            Assert.Same(Theme, result.Asset);
        }
        finally { Directory.Delete(directory, true); }
    }

    // 2. Úspešná HTTP odpoveď → vráti NOVÝ obsah, FromCache = false (cache sa nepoužije)
    [Fact]
    public async Task U6_HttpSuccess_ReturnsFreshContentAndFromCacheFalse()
    {
        string directory = NewCacheDirectory();
        try
        {
            // aj keď existuje stará cache, pri úspešnom HTTP sa nesmie použiť
            await File.WriteAllTextAsync(Path.Combine(directory, Theme.FileName), "OLD");
            using var handler = new ControlledHandler();
            using var client = Client(handler);
            handler.Complete("theme", "NEW");

            DownloadResult result = await new DownloadService(client)
                .DownloadWithCacheAsync(Theme, directory);

            Assert.Equal("NEW", result.Content);
            Assert.False(result.FromCache);
            Assert.Same(Theme, result.Asset);
        }
        finally { Directory.Delete(directory, true); }
    }

    // 3. Zrušenie po začatí požiadavky sa propaguje aj keď cache existuje
    [Fact]
    public async Task U6_Cancellation_AfterRequestStarted_PropagatesEvenWithCache()
    {
        string directory = NewCacheDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(directory, Theme.FileName), "OLD");
            using var handler = new ControlledHandler();
            using var client = Client(handler);
            using var cts = new CancellationTokenSource();

            Task<DownloadResult> task = new DownloadService(client)
                .DownloadWithCacheAsync(Theme, directory, cts.Token);

            // synchronizačný bod: počkáme, kým požiadavka reálne začala
            await handler.StartedAsync("theme");

            // teraz zrušíme — nesmie sa stať, že sa vráti stará cache
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        }
        finally { Directory.Delete(directory, true); }
    }

}

