using System.Diagnostics;
using NeonovyKurier.Core;
using NeonovyKurier.Cli.Game;
using NeonovyKurier.Cli.Infrastructure;

namespace NeonovyKurier.Cli;

// Hotové spustenie pokusov. Učebné metódy sú v Core/DownloadService.cs.
internal static class Scenarios
{
    internal static async Task<int> RunAsync(string command, HttpClient client)
    {
        Console.WriteLine($"\nScenár: {command} (Ctrl+C počas I/O = zrušiť)");
        var service = new DownloadService(client, new ConsoleProgress());
        var clock = Stopwatch.StartNew();
        using var cancel = new CancellationTokenSource();
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; cancel.Cancel(); };
        Console.CancelKeyPress += handler;
        try
        {
            DownloadResult[] results;
            switch (command)
            {
                case "one":
                    Task<DownloadResult> pending = service.DownloadAssetAsync(MissionContent.Assets[0], cancel.Token);
                    Console.WriteLine($"Volajúci má Task, IsCompleted={pending.IsCompleted}");
                    DownloadResult result = await pending;
                    Console.WriteLine(result.Content);
                    break;
                case "sequential":
                    results = await service.DownloadSequentialAsync(MissionContent.Assets, cancel.Token);
                    Print(results); break;
                case "concurrent":
                    results = await service.DownloadConcurrentAsync(MissionContent.Assets, cancel.Token);
                    Print(results); break;
                case "install":
                    results = await service.DownloadConcurrentAsync(MissionContent.Assets, cancel.Token);
                    MissionPackage.Invalidate(MissionContent.InstalledDirectory);
                    await service.SavePackAsync(MissionContent.InstalledDirectory, results, cancel.Token);
                    await MissionPackage.MarkReadyAsync(MissionContent.InstalledDirectory, cancel.Token);
                    Console.WriteLine($"Misia pripravená: {MissionContent.InstalledDirectory}");
                    await GamePreview.CheckAsync(MissionContent.InstalledDirectory);
                    Console.WriteLine("Teraz môžete zadať play alebo spustiť --play.");
                    break;
                case "failure":
                    Task<DownloadResult[]> group = service.DownloadConcurrentAsync(MissionContent.WithFailure, cancel.Token);
                    try { await group; Console.WriteLine("Očakávaná HTTP chyba sa neobjavila."); return 1; }
                    catch (HttpRequestException) { Console.WriteLine($"HTTP 503, Task={group.Status}. WhenAll pozoroval všetky operácie."); }
                    break;
                case "cancel":
                    cancel.CancelAfter(250);
                    await service.DownloadAssetAsync(new("slow", "slow.txt", "/slow", false), cancel.Token);
                    Console.WriteLine("Požiadavka nereagovala na zrušenie."); return 1;
                case "recover":
                    var tasks = new List<Task<DownloadResult>>();
                    foreach (GameAsset asset in MissionContent.WithFailure)
                        tasks.Add(service.DownloadWithCacheAsync(asset, MissionContent.InstalledDirectory, cancel.Token));
                    results = await Task.WhenAll(tasks);
                    Print(results);
                    MissionPackage.Invalidate(MissionContent.InstalledDirectory);
                    await service.SavePackAsync(MissionContent.InstalledDirectory, results, cancel.Token);
                    await MissionPackage.MarkReadyAsync(MissionContent.InstalledDirectory, cancel.Token);
                    await GamePreview.CheckAsync(MissionContent.InstalledDirectory);
                    break;
                default: Console.WriteLine("Neznámy príkaz; použite menu bez argumentov."); return 1;
            }
            Console.WriteLine($"Čas scenára: {clock.ElapsedMilliseconds} ms");
            return 0;
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested)
        {
            Console.WriteLine($"ZRUŠENÉ po {clock.ElapsedMilliseconds} ms. Zrušenie nie je úspešný download.");
            return 0;
        }
        catch (NotImplementedException error) { Console.WriteLine($"NEDOKONČENÉ: {error.Message}"); return 2; }
        catch (Exception error) { Console.WriteLine($"CHYBA: {error.GetType().Name}: {error.Message}"); return 1; }
        finally { Console.CancelKeyPress -= handler; }
    }

    private static void Print(DownloadResult[] results)
        => Console.WriteLine("Výsledné poradie: " + string.Join(", ", results.Select(r =>
            r.Asset.Id + (r.FromCache ? " [CACHE]" : " [HTTP]"))));
}
