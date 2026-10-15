using System.Text.Json;

namespace NeonovyKurier.Core;

public sealed record InstalledMission(MissionDefinition Definition, string Runner, string Hint);

// Hotové načítanie a potvrdenie balíčka. Nie je súčasťou študentských TODO.
public static class MissionPackage
{
    private const string ReadyFile = ".mission-ready";
    private sealed record Theme(string Runner);
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static bool IsReady(string directory) => File.Exists(Path.Combine(directory, ReadyFile))
        && File.Exists(Path.Combine(directory, "level.json"));

    public static void Invalidate(string directory)
    {
        string file = Path.Combine(directory, ReadyFile);
        if (File.Exists(file)) File.Delete(file);
    }

    public static async Task MarkReadyAsync(string directory, CancellationToken cancellationToken = default)
    {
        await ReadAsync(directory, requireReady: false, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(directory, ReadyFile), "Misia bola uložená a overená.", cancellationToken);
    }

    public static async Task<InstalledMission> ReadAsync(string directory, bool requireReady = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (requireReady && !IsReady(directory))
            throw new InvalidDataException("Hlavná misia je zamknutá. Najprv dokonči U2-U4 a stiahni celý balíček.");
        string json = await File.ReadAllTextAsync(Path.Combine(directory, "level.json"), cancellationToken);
        var mission = JsonSerializer.Deserialize<MissionDefinition>(json, Options)
            ?? throw new InvalidDataException("Chýba definícia misie.");
        mission.Validate();
        string runner = "@";
        string theme = Path.Combine(directory, "theme.json");
        if (File.Exists(theme))
        {
            var value = JsonSerializer.Deserialize<Theme>(await File.ReadAllTextAsync(theme, cancellationToken), Options);
            if (value?.Runner is { Length: 1 }) runner = value.Runner;
        }
        string tips = Path.Combine(directory, "tips.txt");
        string hint = File.Exists(tips) ? (await File.ReadAllTextAsync(tips, cancellationToken)).Trim()
            : "Zober všetky zásielky a šetri energiu na povinné skoky.";
        return new(mission, runner, hint);
    }
}
