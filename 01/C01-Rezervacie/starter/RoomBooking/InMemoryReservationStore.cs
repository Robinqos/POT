namespace RoomBooking;

public sealed class InMemoryReservationStore : IReservationStore
{
    private readonly List<Reservation> _reservations = [];

    public IReadOnlyList<Reservation> All() => _reservations.ToArray();

    public Reservation? Find(Guid id) => _reservations.FirstOrDefault(r => r.Id == id);

    public void Add(Reservation reservation) => _reservations.Add(reservation);

    public void Replace(Reservation reservation)
    {
        int index = _reservations.FindIndex(r => r.Id == reservation.Id);
        if (index < 0) throw new InvalidOperationException("Rezervácia neexistuje.");
        _reservations[index] = reservation;
    }
}
