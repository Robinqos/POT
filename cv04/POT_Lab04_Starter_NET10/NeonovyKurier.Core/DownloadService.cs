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

    public async Task<DownloadResult> DownloadAssetAsync(GameAsset asset,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        cancellationToken.ThrowIfCancellationRequested();
        _progress?.Report(new(asset.Id, "START"));
        // BEGIN U2
        // TODO U2: kontrakt a nápoveda sú v NAVOD.md.
        string content = await _client.GetStringAsync(asset.Route, cancellationToken);
        _progress?.Report(new(asset.Id, "DONE"));
        return new DownloadResult(asset, content);
        // END U2
    }

    public async Task<DownloadResult[]> DownloadSequentialAsync(IReadOnlyList<GameAsset> assets,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assets);
        cancellationToken.ThrowIfCancellationRequested();
        // BEGIN U3a
        // TODO U3a: kontrakt a nápoveda sú v NAVOD.md.
        var results = new List<DownloadResult>(assets.Count);
        foreach (var asset in assets)
        {
            var result = await DownloadAssetAsync(asset, cancellationToken);
            results.Add(result);
        }
        return results.ToArray();
        // END U3a
    }

    public async Task<DownloadResult[]> DownloadConcurrentAsync(IReadOnlyList<GameAsset> assets,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assets);
        cancellationToken.ThrowIfCancellationRequested();
        // BEGIN U3b
        // TODO U3b: kontrakt a nápoveda sú v NAVOD.md.
        var tasks = new List<Task<DownloadResult>>(assets.Count);
        foreach (var asset in assets)
        {
            tasks.Add(DownloadAssetAsync(asset, cancellationToken));
        }
        return await Task.WhenAll(tasks);
        // END U3b
    }

    public async Task SavePackAsync(string directory, IReadOnlyList<DownloadResult> results,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(directory);
        // BEGIN U4
        // TODO U4: kontrakt a nápoveda sú v NAVOD.md.
        foreach (var result in results)
        {
            string path = PackageFiles.PathFor(directory, result.Asset);
            await File.WriteAllTextAsync(path, result.Content, cancellationToken);
        }
        // END U4
    }

    /// <summary>U6: len pri HTTP výpadku voliteľného súboru použije existujúcu cache.</summary>
    public async Task<DownloadResult> DownloadWithCacheAsync(GameAsset asset, string directory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        string file = PackageFiles.PathFor(directory, asset);
        // BEGIN U6
        // TODO U6: kontrakt a nápoveda sú v NAVOD.md.
        try
        {
            return await DownloadAssetAsync(asset, cancellationToken);
        }
        catch (HttpRequestException) when (!asset.Required && File.Exists(file))
        {
            string cached = await File.ReadAllTextAsync(file, cancellationToken);
            _progress?.Report(new(asset.Id, "CACHE"));
            return new DownloadResult(asset, cached, true);
        }
        // END U6
    }

    public async Task<DownloadResult[]> DownloadLimitedAsync(IReadOnlyList<GameAsset> assets,
        int maxConcurrency, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxConcurrency);
        cancellationToken.ThrowIfCancellationRequested();
        // BEGIN U7
        // TODO U7: kontrakt a nápoveda sú v NAVOD.md.
        using var slots = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var tasks = new List<Task<DownloadResult>>(assets.Count);
        foreach (var asset in assets)
        {
            tasks.Add(DownloadOneAsync(asset));
        }
        return await Task.WhenAll(tasks);

        async Task<DownloadResult> DownloadOneAsync(GameAsset asset)
        {
            await slots.WaitAsync(cancellationToken);
            try
            {
                return await DownloadAssetAsync(asset, cancellationToken);
            }
            finally
            {
                slots.Release();
            }
        }
        // END U7
    }
}
