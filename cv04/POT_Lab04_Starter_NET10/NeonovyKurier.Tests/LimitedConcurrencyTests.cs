using System.Net;
using NeonovyKurier.Core;

namespace NeonovyKurier.Tests;

// U7: sem pridajte vlastné testy DownloadLimitedAsync. Názvy začínajte U7_.
// Práca s ControlledHandler je opísaná v OwnFeatureTests.cs.
public sealed class LimitedConcurrencyTests
{
    private static readonly GameAsset[] Assets =
    [
        new("level", "level.json", "/level", Required: true),
        new("theme", "theme.json", "/theme", Required: false),
        new("tips", "tips.txt", "/tips", Required: false)
    ];

    private static HttpClient Client(ControlledHandler handler)
        => new(handler) { BaseAddress = new Uri("http://localhost/") };

    // Kostra prvého testu. Odkomentujte ju a doplňte časti označené "...".
    // handler.Requests obsahuje identifikátory už začatých HTTP požiadaviek v poradí začatia.
    //
    // [Fact]
    // public async Task U7_LimitTwo_StartsThirdOnlyAfterSlotIsReleased()
    // {
    //     using var handler = new ControlledHandler();
    //     using var client = Client(handler);
    //     using var cancel = new CancellationTokenSource();
    //     Task<DownloadResult[]> work = new DownloadService(client).DownloadLimitedAsync(Assets, 2, cancel.Token);
    //     try
    //     {
    //         await Task.WhenAll(handler.StartedAsync("level"), handler.StartedAsync("theme"));
    //         Assert.Equal(new[] { "level", "theme" }, handler.Requests); // tips ešte nezačal
    //         handler.Complete("theme", "THEME");
    //         ...
    //     }
    //     finally
    //     {
    //         // Aj pri zlyhaní testu ukončite rozpracované operácie.
    //         cancel.Cancel();
    //         try { await work; } catch (OperationCanceledException) { } catch (HttpRequestException) { }
    //     }
    // }
}
