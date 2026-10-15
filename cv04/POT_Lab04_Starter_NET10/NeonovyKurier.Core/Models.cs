namespace NeonovyKurier.Core;

/// <summary>Súbor malého textového herného balíčka a jeho HTTP cesta.</summary>
public sealed record GameAsset(string Id, string FileName, string Route, bool Required);

/// <summary>Hotový obsah; FromCache rozlišuje aktuálnu odpoveď od lokálnej kópie.</summary>
public sealed record DownloadResult(GameAsset Asset, string Content, bool FromCache = false);

/// <summary>Oznámenie priebehu; nejde o percentá prenesených bajtov.</summary>
public sealed record DownloadProgress(string AssetId, string Stage);
