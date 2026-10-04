namespace RoomBooking;

public static class SelfTests
{
    public static int Run()
    {
        int failures = 0;
        Check("bežná rezervácia", () => NewService().Book("A101", "Anna", Slot(9, 10)).Status == ReservationStatus.Success);

        Check("susedné intervaly sa môžu dotýkať", () =>
        {
            var service = NewService();
            service.Book("A101", "Anna", Slot(9, 10));
            return service.Book("A101", "Boris", Slot(10, 11)).Status == ReservationStatus.Success;
        });

        Check("susedné intervaly sa môžu dotýkať aj v opačnom poradí", () =>
        {
            var service = NewService();
            service.Book("A101", "Anna", Slot(10, 11));
            return service.Book("A101", "Boris", Slot(9, 10)).Status == ReservationStatus.Success;
        });

        Check("skutočný prekryv je konflikt", () =>
        {
            var service = NewService();
            service.Book("A101", "Anna", Slot(9, 10));
            return service.Book("A101", "Boris", Slot(9.5, 10.5)).Status == ReservationStatus.Conflict;
        });

        Check("rôzne miestnosti sa neblokujú", () =>
        {
            var service = NewService();
            service.Book("A101", "Anna", Slot(9, 10));
            return service.Book("B202", "Boris", Slot(9, 10)).Status == ReservationStatus.Success;
        });

        Check("presun na voľný čas", () =>
        {
            var store = new InMemoryReservationStore();
            var service = new ReservationService(store);
            var booked = service.Book("A101", "Anna", Slot(9, 10)).Reservation!;
            var moved = service.Move(booked.Id, Slot(11, 12));
            return moved.Status == ReservationStatus.Success &&
                   moved.Reservation!.Id == booked.Id &&
                   store.Find(booked.Id)!.Time == Slot(11, 12);
        });

        Check("konfliktný presun ponechá pôvodný stav", () =>
        {
            var store = new InMemoryReservationStore();
            var service = new ReservationService(store);
            var first = service.Book("A101", "Anna", Slot(9, 10)).Reservation!;
            service.Book("A101", "Boris", Slot(11, 12));
            var result = service.Move(first.Id, Slot(11, 12));
            return result.Status == ReservationStatus.Conflict && store.Find(first.Id)!.Time == Slot(9, 10);
        });

        Check("presun na rovnaký čas je úspešný", () =>
        {
            var service = NewService();
            var booked = service.Book("A101", "Anna", Slot(9, 10)).Reservation!;
            return service.Move(booked.Id, Slot(9, 10)).Status == ReservationStatus.Success;
        });

        Check("neexistujúca rezervácia", () =>
            NewService().Move(Guid.NewGuid(), Slot(9, 10)).Status == ReservationStatus.NotFound);

        Console.WriteLine(failures == 0 ? "Všetky kontroly prešli." : $"Neúspešné kontroly: {failures}");
        return failures == 0 ? 0 : 1;

        void Check(string name, Func<bool> run)
        {
            try
            {
                bool ok = run();
                Console.WriteLine($"{(ok ? "OK" : "FAIL")}: {name}");
                if (!ok) failures++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAIL: {name} ({ex.GetType().Name})");
                failures++;
            }
        }
    }

    private static ReservationService NewService() => new(new InMemoryReservationStore());

    private static TimeRange Slot(double startHour, double endHour)
    {
        DateTime day = new(2026, 10, 1);
        return new TimeRange(day.AddHours(startHour), day.AddHours(endHour));
    }
}
