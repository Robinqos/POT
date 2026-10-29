namespace FileDetective.Core;

public interface IFileReader
{
    Task<byte[]> ReadAsync(string path, CancellationToken token);
}

public sealed class FileReader : IFileReader
{
    public const int MaxFileBytes = 32 * 1024 * 1024;

    // BEGIN U1
    public async Task<byte[]> ReadAsync(string path, CancellationToken token)
    {
        return await ReadBoundedAsync(path, token).ConfigureAwait(false);
    }
    // END U1

    // Hotové čítanie s limitom pamäte. Nejde o cieľ úlohy.
    private static async Task<byte[]> ReadBoundedAsync(string path, CancellationToken token)
    {
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            65536, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (input.Length > MaxFileBytes) throw new IOException("Súbor je väčší než limit 32 MiB.");
        using var output = new MemoryStream();
        byte[] buffer = new byte[65536];
        int read;
        while ((read = await input.ReadAsync(buffer, token).ConfigureAwait(false)) != 0)
        {
            if (output.Length + read > MaxFileBytes) throw new IOException("Súbor prekročil limit 32 MiB.");
            output.Write(buffer, 0, read);
        }
        token.ThrowIfCancellationRequested();
        return output.ToArray();
    }
}
