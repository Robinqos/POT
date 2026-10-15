namespace NeonovyKurier.Core;

/// <summary>HTTP a súborové I/O. HttpClient vlastní volajúci, nie táto služba.</summary>
public sealed class DownloadService
{
    private readonly HttpClient _client;
    private readonly IProgress<DownloadProgress>? _progress;

    public DownloadService(HttpClient client, IProgress<DownloadProgress>? progress = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _progress = progress;
    }

    public Task<DownloadResult> DownloadAssetAsync(GameAsset asset,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        cancellationToken.ThrowIfCancellationRequested();
        _progress?.Report(new(asset.Id, "START"));
        // BEGIN U2
        // TODO U2: kontrakt a nápoveda sú v NAVOD.md.
        throw new NotImplementedException("U2 DownloadAssetAsync (Core/DownloadService.cs)");
        // END U2
    }

    public Task<DownloadResult[]> DownloadSequentialAsync(IReadOnlyList<GameAsset> assets,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assets);
        cancellationToken.ThrowIfCancellationRequested();
        // BEGIN U3a
        // TODO U3a: kontrakt a nápoveda sú v NAVOD.md.
        throw new NotImplementedException("U3a DownloadSequentialAsync (Core/DownloadService.cs)");
        // END U3a
    }

    public Task<DownloadResult[]> DownloadConcurrentAsync(IReadOnlyList<GameAsset> assets,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assets);
        cancellationToken.ThrowIfCancellationRequested();
        // BEGIN U3b
        // TODO U3b: kontrakt a nápoveda sú v NAVOD.md.
        throw new NotImplementedException("U3b DownloadConcurrentAsync (Core/DownloadService.cs)");
        // END U3b
    }

    public Task SavePackAsync(string directory, IReadOnlyList<DownloadResult> results,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(directory);
        // BEGIN U4
        // TODO U4: kontrakt a nápoveda sú v NAVOD.md.
        throw new NotImplementedException("U4 SavePackAsync (Core/DownloadService.cs)");
        // END U4
    }

    /// <summary>U6: len pri HTTP výpadku voliteľného súboru použije existujúcu cache.</summary>
    public Task<DownloadResult> DownloadWithCacheAsync(GameAsset asset, string directory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        string file = PackageFiles.PathFor(directory, asset);
        // BEGIN U6
        // TODO U6: kontrakt a nápoveda sú v NAVOD.md.
        throw new NotImplementedException("U6 DownloadWithCacheAsync (Core/DownloadService.cs)");
        // END U6
    }

    public Task<DownloadResult[]> DownloadLimitedAsync(IReadOnlyList<GameAsset> assets,
        int maxConcurrency, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxConcurrency);
        cancellationToken.ThrowIfCancellationRequested();
        // BEGIN U7
        // TODO U7: kontrakt a nápoveda sú v NAVOD.md.
        throw new NotImplementedException("U7 DownloadLimitedAsync (Core/DownloadService.cs)");
        // END U7
    }
}
