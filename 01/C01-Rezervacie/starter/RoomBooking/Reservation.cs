namespace RoomBooking;

public sealed record Reservation(Guid Id, string Room, string Owner, TimeRange Time);

public enum ReservationStatus
{
    Success,
    Conflict,
    InvalidRange,
    NotFound
}

public sealed record ReservationResult(ReservationStatus Status, Reservation? Reservation);
