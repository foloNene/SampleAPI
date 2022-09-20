using Microsoft.EntityFrameworkCore;
using SampleApi.Contexts;
using SampleApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    public class BookRepository : IBookRepository, IDisposable
    {
        private LibraryContext _context;
        public BookRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentException(nameof(authorId));
            }

            return await _context.Books
             .Include(b => b.Author)
               .Where(b => b.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<Book> GetBookAsync(Guid authorId, Guid bookId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentException(nameof(authorId));
            }

            if(bookId == Guid.Empty)
            {
                throw new ArgumentException(nameof(bookId));
            }

            return await _context.Books
              .Include(b => b.Author)
                 .Where(b => b.AuthorId == authorId && b.Id == bookId)
                    .FirstOrDefaultAsync();

        }

        //just book
        public void AddBook(Book bookToAdd)
        {
            if (bookToAdd == null)
            {
                throw new ArgumentNullException(nameof(bookToAdd));
            }

            _context.Add(bookToAdd);
        }

        //authorId and Boo
        public void AddBook(Guid authorId, Book book)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            book.AuthorId = authorId;
            _context.Books.Add(book);

        }

        public void UpdateBook(Book book)
        {
            // no code in this implementation
        }


        public async Task<bool> AuthorExistsAsync(Guid authorId)
        {
            return await _context.Authors.AnyAsync(a => a.Id == authorId);
        }


        public void DeleteBook(Book book)
        {
            _context.Books.Remove(book);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

    }
}
