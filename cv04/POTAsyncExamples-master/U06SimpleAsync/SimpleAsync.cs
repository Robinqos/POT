namespace KST.POT.AsyncExamples.U06SimpleAsync;

public static class SimpleAsync
{
    public static async Task DoIt()
    {
        Console.WriteLine("Som tu");
        await Task.Delay(1000);
        Console.WriteLine("A tu");
        await Task.Delay(2000);
        Console.WriteLine("A este tu");
    }
}
