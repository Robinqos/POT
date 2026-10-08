namespace PlaylistLab.Core.Models;

/// <summary>Nemenné údaje o skladbe. Zvukový súbor nie je súčasťou modelu.</summary>
public sealed record Track : IIdentifiable
{
    /// <summary>Identifikátor skladby.</summary>
    public string Id { get; }
    /// <summary>Názov skladby.</summary>
    public string Title { get; }
    /// <summary>Meno interpreta.</summary>
    public string Artist { get; }
    /// <summary>Album alebo null, ak údaj chýba.</summary>
    public string? Album { get; }
    /// <summary>Kladná dĺžka skladby v sekundách.</summary>
    public int DurationSeconds { get; }

    /// <summary>Vytvorí platnú skladbu.</summary>
    /// <param name="id">Neprázdne ID.</param>
    /// <param name="title">Neprázdny názov.</param>
    /// <param name="artist">Neprázdne meno interpreta.</param>
    /// <param name="durationSeconds">Kladná dĺžka v sekundách.</param>
    /// <param name="album">Voliteľný album.</param>
    /// <exception cref="ArgumentException">Textový povinný údaj je prázdny.</exception>
    /// <exception cref="ArgumentNullException">Povinný textový údaj je null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Dĺžka nie je kladná.</exception>
    public Track(string id, string title, string artist, int durationSeconds, string? album = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(artist);
        if (durationSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Dĺžka musí byť kladná.");
        (Id, Title, Artist, DurationSeconds, Album) = (id, title, artist, durationSeconds, album);
    }
}
