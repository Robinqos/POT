using System.Text;
using NeonovyKurier.Cli;
using NeonovyKurier.Cli.Game;
using NeonovyKurier.Cli.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;
string mode = (args.FirstOrDefault() ?? "menu").TrimStart('-');
try
{
    if (mode == "trace") { await TraceExperiment.RunAsync(); return 0; }
    if (mode == "training")
    {
        Console.WriteLine("TRÉNING ovládania. Hlavná misia sa sprístupní až po U2-U4.");
        await GamePreview.TrainAsync();
        return 0;
    }
    if (mode == "play") { await GamePreview.PlayAsync(MissionContent.InstalledDirectory); return 0; }
    if (mode == "check") { await GamePreview.CheckAsync(MissionContent.InstalledDirectory); return 0; }
    await using var server = await DemoServer.StartAsync();
    using var client = new HttpClient { BaseAddress = server.BaseAddress };
    Console.WriteLine("NEÓNOVÝ KURIÉR - najprv získať misiu, potom doručiť zásielky.");
    if (mode == "basic")
    {
        int incomplete = 0, errors = 0;
        foreach (string command in new[] { "one", "sequential", "concurrent", "install" })
        {
            int code = await Scenarios.RunAsync(command, client);
            if (code == 2) incomplete++; else if (code != 0) errors++;
        }
        Console.WriteLine($"Kontrola: {incomplete} nedokončené scenáre, {errors} neočakávané chyby.");
        return errors > 0 ? 1 : 0;
    }
    if (mode != "menu") return await Scenarios.RunAsync(mode, client);
    Console.WriteLine("Príkazy: one, sequential, concurrent, install, failure, cancel, recover, play, exit");
    while (true)
    {
        Console.Write("kurier> ");
        string? command = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (command is null or "exit") return 0;
        if (command == "play")
        {
            try { await GamePreview.PlayAsync(MissionContent.InstalledDirectory); }
            catch (Exception error) { Console.WriteLine($"Najprv použite install. {error.Message}"); }
        }
        else await Scenarios.RunAsync(command, client);
    }
}
catch (Exception error)
{
    Console.Error.WriteLine($"CHYBA: {error.GetType().Name}: {error.Message}");
    return 1;
}
