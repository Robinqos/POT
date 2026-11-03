using System.Text;
using FileDetective.Core;

namespace FileDetective.Tests;

internal sealed class TestFolder : IDisposable
{
    public string Root { get; } = Path.Combine(AppContext.BaseDirectory, "test-data", Guid.NewGuid().ToString("N"));
    public TestFolder() => Directory.CreateDirectory(Root);
    public string Put(string name, string text) => Put(name, Encoding.UTF8.GetBytes(text));
    public string Put(string name, byte[] bytes)
    {
        string path = Path.Combine(Root, name); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes); return path;
    }
    public void Dispose()
    {
        if (!Path.GetFullPath(Root).StartsWith(Path.GetFullPath(AppContext.BaseDirectory), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Nesprávny priečinok testu.");
        Directory.Delete(Root, true);
    }
}

internal sealed class InlineProgress<T>(Action<T> action) : IProgress<T>
{
    public void Report(T value) => action(value);
}

internal sealed class ControlledReader : IFileReader
{
    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<byte[]> _result = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public void Complete() => _result.TrySetResult([1, 2, 3]);
    public async Task<byte[]> ReadAsync(string path, CancellationToken token)
    { Started.TrySetResult(); return await _result.Task.WaitAsync(token); }
}

internal sealed class BrokenReader : IFileReader
{
    public Task<byte[]> ReadAsync(string path, CancellationToken token) => Task.FromException<byte[]>(new IOException("Nedostupný súbor"));
}

internal sealed class ControlledScanner : IScanner
{
    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<ScanReport> _result = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public ScanReport Report { get; } = new("root", 0, 0, [], []);
    public bool Ended { get; private set; }
    public void Complete() => _result.TrySetResult(Report);
    public async Task<ScanReport> ScanAsync(string root, IProgress<ScanProgress>? progress, CancellationToken token)
    {
        Started.TrySetResult();
        try { return await _result.Task.WaitAsync(token); }
        finally { Ended = true; }
    }
}

internal static class Cleanup
{
    public static async Task Observe(Task? work)
    {
        if (work == null) return;
        try { await work.WaitAsync(TimeSpan.FromSeconds(5)); }
        catch (Exception) { /* Zachovať pôvodnú chybu tvrdenia testu. */ }
    }
}
