using System.Collections.ObjectModel;

namespace WpfUkazky.ViewModels;

// slajd 31 – dáta vo ViewModeli namiesto XmlDataProvider
public class Book
{
    public string Isbn { get; init; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public int Number { get; init; }
}

public class LibraryViewModel
{
    public ObservableCollection<Book> Books { get; } =
    [
        new() { Isbn = "0-7356-0562-9", Number = 9, Title = "XML in Action",
                Summary = "XML Web Technology" },
        new() { Isbn = "0-7356-1370-2", Number = 8,
                Title = "Programming Microsoft Windows With C#",
                Summary = "C# Programming using the .NET Framework" },
        new() { Isbn = "0-7356-1288-9", Number = 7, Title = "Inside C#",
                Summary = "C# Language Programming" },
        new() { Isbn = "0-7356-1377-X", Number = 5, Title = "Introducing Microsoft .NET",
                Summary = "Overview of .NET Technology" },
        new() { Isbn = "0-7356-1448-2", Number = 4, Title = "Microsoft C# Language Specifications",
                Summary = "The C# language definition" },
    ];
}
