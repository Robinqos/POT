namespace RoomBooking;

// Interval je polootvorený: začiatok patrí do rezervácie, koniec už nie.
public sealed record TimeRange(DateTime Start, DateTime End)
{
    public bool IsValid => End > Start;

    public bool Overlaps(TimeRange other)
    {
        return Start <= other.End && other.Start <= End;
    }
}
