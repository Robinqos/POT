namespace RoomBooking;

public interface IReservationStore
{
    IReadOnlyList<Reservation> All();
    Reservation? Find(Guid id);
    void Add(Reservation reservation);
    void Replace(Reservation reservation);
}
