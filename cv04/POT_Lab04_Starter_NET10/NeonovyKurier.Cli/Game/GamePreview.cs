using NeonovyKurier.Core;

namespace NeonovyKurier.Cli.Game;

// Rovnaké pravidlá ako v grafickom okne. Len záložné konzolové ovládanie.
internal static class GamePreview
{
    internal static async Task CheckAsync(string directory)
    {
        var data = await MissionPackage.ReadAsync(directory);
        Console.WriteLine($"MISIA OK: {data.Definition.Name}, {data.Definition.Track.Length} políčok, energia {data.Definition.JumpBudget}.");
    }

    internal static Task TrainAsync()
    {
        Play(MissionDefinition.Training, "@", isTraining: true);
        return Task.CompletedTask;
    }

    internal static async Task PlayAsync(string directory)
    {
        var data = await MissionPackage.ReadAsync(directory);
        Play(data.Definition, data.Runner, isTraining: false);
    }

    private static void Play(MissionDefinition definition, string runner, bool isTraining)
    {
        var session = new MissionSession(definition, isTraining);
        Console.WriteLine($"\n{definition.Name}: Enter=krok, j=skok, r=od začiatku, q=koniec. *=povinná zásielka.");
        while (true)
        {
            Console.WriteLine(definition.Track[..session.Position] + runner + definition.Track[(session.Position + 1)..]);
            Console.WriteLine($"Zásielky {session.Collected.Count}/{session.TotalParcels}; energia {session.JumpsLeft}/{definition.JumpBudget}. {session.Message}");
            if (session.Won) return;
            Console.Write("> ");
            string? command = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (command is null or "q") return;
            if (command == "r") { session.Reset(); continue; }
            if (command is not ("" or "j")) { Console.WriteLine("Použite Enter, j, r alebo q."); continue; }
            session.Move(command == "j" ? 2 : 1);
        }
    }
}
