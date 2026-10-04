using RoomBooking;

// Aby sa v konzole správne zobrazila diakritika a znak →.
Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args is ["--self-test"])
    return SelfTests.Run();

var store = new InMemoryReservationStore();
var service = new ReservationService(store);
DateTime day = new(2026, 10, 1);

Show("A101, 09:00–10:00", service.Book("A101", "Anna", new TimeRange(day.AddHours(9), day.AddHours(10))));
Show("A101, 10:00–11:00", service.Book("A101", "Boris", new TimeRange(day.AddHours(10), day.AddHours(11))));
Show("A101, 09:30–10:30", service.Book("A101", "Cyril", new TimeRange(day.AddHours(9.5), day.AddHours(10.5))));
Show("B202, 09:30–10:30", service.Book("B202", "Dana", new TimeRange(day.AddHours(9.5), day.AddHours(10.5))));
return 0;

static void Show(string label, ReservationResult result) =>
    Console.WriteLine($"{label,-24} → {result.Status}");
