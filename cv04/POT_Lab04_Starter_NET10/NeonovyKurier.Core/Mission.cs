namespace NeonovyKurier.Core;

// Pripravené pravidlá hry. Študent dopĺňa iba DownloadService.
public sealed record MissionDefinition(string Name, string Track, int JumpBudget)
{
    public static MissionDefinition Training { get; } = new("Tréning ovládania", "S.*G", 1);

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || Track is null || Track.Length is < 4 or > 40 ||
            !Track.StartsWith('S') || !Track.EndsWith('G') || Track[1..^1].Any(c => !".#*".Contains(c)) ||
            !Track.Contains('*') || JumpBudget is < 0 or > 20)
            throw new InvalidDataException("Misia potrebuje názov, 4-40 políčok S . # * G, aspoň jednu zásielku a jumpBudget 0-20.");

        // Pohyb ide iba doprava. Povinnú zásielku nemožno preskočiť.
        int[] jumps = Enumerable.Repeat(int.MaxValue, Track.Length).ToArray();
        jumps[0] = 0;
        for (int p = 0; p < Track.Length; p++)
        {
            if (jumps[p] == int.MaxValue || Track[p] == '#') continue;
            foreach (int distance in new[] { 1, 2 })
            {
                int next = Math.Min(p + distance, Track.Length - 1);
                if (next == p || Track[next] == '#' || distance == 2 && Track[p + 1] == '*') continue;
                jumps[next] = Math.Min(jumps[next], jumps[p] + (distance == 2 ? 1 : 0));
            }
        }
        if (jumps[^1] > JumpBudget)
            throw new InvalidDataException("Misia nie je priechodná so všetkými zásielkami a zadanou energiou. Uprav mapu alebo jumpBudget.");
    }
}

public sealed class MissionSession
{
    public MissionDefinition Mission { get; }
    public bool IsTraining { get; }
    public int Position { get; private set; }
    public int JumpsUsed { get; private set; }
    public int JumpsLeft => Mission.JumpBudget - JumpsUsed;
    public HashSet<int> Collected { get; } = [];
    public int TotalParcels => Mission.Track.Count(c => c == '*');
    public bool Finished { get; private set; }
    public bool Won { get; private set; }
    public string Message { get; private set; } = "";

    public MissionSession(MissionDefinition mission, bool isTraining = false)
    {
        mission.Validate(); Mission = mission; IsTraining = isTraining; Reset();
    }

    public void Reset()
    {
        Position = 0; JumpsUsed = 0; Collected.Clear(); Finished = Won = false;
        Message = "Zober každú žltú zásielku. Skáč iba tam, kde to potrebuješ.";
    }

    public void Move(int distance)
    {
        if (Finished) return;
        if (distance is not (1 or 2)) throw new ArgumentOutOfRangeException(nameof(distance));
        if (distance == 2)
        {
            if (JumpsLeft == 0) { Lose("Došla energia na skok. Skús trasu s menším počtom skokov."); return; }
            JumpsUsed++;
            if (Mission.Track[Position + 1] == '*')
            {
                Position = Math.Min(Position + distance, Mission.Track.Length - 1);
                Lose("Preskočil si povinnú zásielku. Vrátiť sa nedá; začni znova."); return;
            }
        }
        Position = Math.Min(Position + distance, Mission.Track.Length - 1);
        if (Mission.Track[Position] == '#') { Lose("Dopadol si na stenu. Pred stenou použi skok."); return; }
        if (Mission.Track[Position] == '*') Collected.Add(Position);
        if (Position == Mission.Track.Length - 1)
        {
            if (Collected.Count != TotalParcels) { Lose("Chýba zásielka. Zober všetky a až potom doruč do cieľa."); return; }
            Finished = Won = true;
            Message = IsTraining
                ? "Tréning hotový. Hlavnú misiu sprístupníš stiahnutím a uložením balíčka v U2-U4."
                : $"Misia splnená! Doručené {Collected.Count}/{TotalParcels} zásielok, skoky {JumpsUsed}/{Mission.JumpBudget}.";
        }
        else Message = Mission.Track[Position] == '*'
            ? $"Zásielka vyzdvihnutá: {Collected.Count}/{TotalParcels}."
            : "Pozri ďalšie políčka. Zásielky nesmieš preskočiť a energia je obmedzená.";
    }

    private void Lose(string message) { Finished = true; Won = false; Message = message + " Stlač Od začiatku."; }
}
