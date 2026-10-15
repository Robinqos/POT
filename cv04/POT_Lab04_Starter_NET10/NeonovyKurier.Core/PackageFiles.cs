namespace NeonovyKurier.Core;

// Pripravená práca s názvom. Balíček používa súbory priamo v jednom priečinku.
public static class PackageFiles
{
    public static string PathFor(string directory, GameAsset asset)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(asset);
        string name = asset.FileName;
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || name.IndexOfAny(['/', '\\']) >= 0)
            throw new ArgumentException("FileName musí byť jednoduchý názov súboru.", nameof(asset));
        return Path.Combine(directory, name);
    }
}
