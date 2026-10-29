using System.Security.Cryptography;

namespace FileDetective.Core;

public static class Fingerprint
{
    public const int ChunkSize = 65536;

    // Pripravený CPU algoritmus. Doplňte iba kontrolný bod rušenia U2a.
    public static string Compute(byte[] bytes, CancellationToken token, Action<int>? checkpoint = null)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        token.ThrowIfCancellationRequested();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for (int offset = 0; offset < bytes.Length; offset += ChunkSize)
        {
            // BEGIN U2A
            // TODO U2a: overte token pri každom bloku CPU výpočtu.
            // END U2A
            int length = Math.Min(ChunkSize, bytes.Length - offset);
            hash.AppendData(bytes, offset, length);
            checkpoint?.Invoke(offset + length);
        }
        token.ThrowIfCancellationRequested();
        return Convert.ToHexString(hash.GetHashAndReset());
    }
}
