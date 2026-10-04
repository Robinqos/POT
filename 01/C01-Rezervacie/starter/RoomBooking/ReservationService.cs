namespace RoomBooking;

public sealed class ReservationService(IReservationStore store)
{
    public ReservationResult Book(string room, string owner, TimeRange time)
    {
        if (!time.IsValid)
            return new ReservationResult(ReservationStatus.InvalidRange, null);

        if (HasConflict(room, time, null))
            return new ReservationResult(ReservationStatus.Conflict, null);

        var reservation = new Reservation(Guid.NewGuid(), room, owner, time);
        store.Add(reservation);
        return new ReservationResult(ReservationStatus.Success, reservation);
    }

    public ReservationResult Move(Guid id, TimeRange newTime)
    {
        // TODO: implementujte bezpečný presun rezervácie podľa zadania.
        throw new NotImplementedException();
    }

    private bool HasConflict(string room, TimeRange time, Guid? exceptId)
    {
        return store.All().Any(existing =>
            existing.Id != exceptId &&
            existing.Room.Equals(room, StringComparison.OrdinalIgnoreCase) &&
            existing.Time.Overlaps(time));
    }
}
