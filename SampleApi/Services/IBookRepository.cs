using SampleApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetBooksAsync(Guid authorId);

        Task<Book> GetBookAsync(Guid authorId, Guid bookId);

        void AddBook(Book bookToAdd);

        void AddBook(Guid authorId, Book bookToAdd);

        Task<bool> SaveChangesAsync();

        void UpdateBook(Book book);

        void DeleteBook(Book book);

        Task<bool> AuthorExistsAsync(Guid authorId);
    }
}
