namespace NeonovyKurier.Cli;

internal static class TraceExperiment
{
    internal static async Task RunAsync()
    {
        Console.WriteLine("A pred zavolaním");
        Task<string> work = ReadAsync();
        Console.WriteLine($"C Task získaný, IsCompleted={work.IsCompleted}");
        string result = await work;
        Console.WriteLine($"E výsledok={result}, Status={work.Status}");

        int before = Environment.CurrentManagedThreadId;
        await Task.FromResult("hotová cache").ConfigureAwait(false);
        Console.WriteLine($"Dokončený Task + false: thread pred={before}, po={Environment.CurrentManagedThreadId}");
        Console.WriteLine($"SynchronizationContext={SynchronizationContext.Current?.GetType().Name ?? "null"}");
    }

    private static async Task<string> ReadAsync()
    {
        Console.WriteLine("B metóda pred await");
        await Task.Delay(500); // U1: dočasne zmeňte na 0 a predpovedzte poradie.
        Console.WriteLine("D metóda po await");
        return "level pripravený";
    }
}
