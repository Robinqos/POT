using System.Diagnostics;
using NeonovyKurier.Core;

namespace NeonovyKurier.Cli.Infrastructure;

// Synchrónny a krátky odberateľ; Console.WriteLine je synchronizované volanie.
// Core pozná iba IProgress, preto nemusí poznať konzolu alebo UI.
internal sealed class ConsoleProgress : IProgress<DownloadProgress>
{
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    public void Report(DownloadProgress value)
        => Console.WriteLine($"{_clock.ElapsedMilliseconds,5} ms  {value.Stage,-5} {value.AssetId}");
}
