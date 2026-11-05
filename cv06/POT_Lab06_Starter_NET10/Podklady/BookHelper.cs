using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication.HelperLib.Books
{
    record Book(Guid Id, string Title, string Author, int Year);
    class InMemoryBooks
    {
        //...
        private readonly List<Book> _books = new()
    {
        new(Guid.NewGuid(), "Clean Code", "Robert C. Martin", 2008),
        new(Guid.NewGuid(), "CLR via C#", "Jeffrey Richter", 2012)
    };

        public IEnumerable<Book> Query(string? q) =>
            string.IsNullOrWhiteSpace(q) ? _books :
            _books.Where(b => b.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                           || b.Author.Contains(q, StringComparison.OrdinalIgnoreCase));

        public bool TryGet(Guid id, out Book? b) { b = _books.FirstOrDefault(x => x.Id == id); return b is not null; }
        public Book Add(CreateBookDto dto)
        {
            var b = new Book(Guid.NewGuid(), dto.Title!, dto.Author!, dto.Year!.Value);
            _books.Add(b); return b;
        }
        public bool Update(Guid id, UpdateBookDto dto)
        {
            var idx = _books.FindIndex(x => x.Id == id);
            if (idx < 0) return false;
            var current = _books[idx];
            _books[idx] = current with
            {
                Title = dto.Title ?? current.Title,
                Author = dto.Author ?? current.Author,
                Year = dto.Year ?? current.Year
            };
            return true;
        }
        public bool Delete(Guid id) => _books.RemoveAll(x => x.Id == id) > 0;

        public static (IEnumerable<Book> items, int total) Paginate(IEnumerable<Book> src, int page, int size)
        {
            var total = src.Count();
            var items = src.Skip((page - 1) * size).Take(size);
            return (items, total);
        }
    }

    class CreateBookDto
    {
        [Required, MinLength(2)] public string? Title { get; set; }
        [Required, MinLength(2)] public string? Author { get; set; }
        [Range(1450, 2100)] public int? Year { get; set; }
    }
    class UpdateBookDto
    {
        [MinLength(2)] public string? Title { get; set; }
        [MinLength(2)] public string? Author { get; set; }
        [Range(1450, 2100)] public int? Year { get; set; }
    }
}
