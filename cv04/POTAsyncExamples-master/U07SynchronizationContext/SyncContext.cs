namespace KST.POT.AsyncExamples.U07SynchronizationContext;

public class SyncContext
{
    public static void Show()
    {
        Console.WriteLine("SynchronizationContext.Current = " + (SynchronizationContext.Current?.ToString() ?? "(null)"));
    }
}
