using NeonovyKurier.Core;

namespace NeonovyKurier.Cli;

internal static class MissionContent
{
    internal static string SourceDirectory => Path.Combine(AppContext.BaseDirectory, "Content");
    internal static string InstalledDirectory => Path.Combine(AppContext.BaseDirectory, "installed");
    internal static GameAsset[] Assets =>
    [
        new("level", "level.json", "/level", Required: true),
        new("theme", "theme.json", "/theme", Required: false),
        new("tips", "tips.txt", "/tips", Required: false)
    ];
    internal static GameAsset[] WithFailure =>
    [
        Assets[0], Assets[1] with { Route = "/failure" }, Assets[2]
    ];
}
