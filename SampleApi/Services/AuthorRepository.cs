using Microsoft.EntityFrameworkCore;
using SampleApi.Contexts;
using SampleApi.Entities;
using SampleApi.ResourceParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    public class AuthorRepository : IAuthorRepository, IDisposable
    {
        private LibraryContext _context;

        public AuthorRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<bool> AuthorExistsAsync(Guid authorId)
        {
            return await _context.Authors.
                AnyAsync(a => a.Id == authorId);
        }

        public async Task<IEnumerable<Author>> GetAuthorsAsync()
        {
            return await _context.Authors.ToListAsync();
        }

        //Filtering and Searching
        public async Task<IEnumerable<Author>> GetAuthorsAsync(AuthorsResourceParameters authorsResourceParameters)
        {

            if (authorsResourceParameters == null)
            {
                throw new ArgumentNullException(nameof(authorsResourceParameters));
            }

            if (string.IsNullOrWhiteSpace(authorsResourceParameters.MainCategory)
                && string.IsNullOrWhiteSpace(authorsResourceParameters.SearchQuery))
            {
                return await _context.Authors.ToListAsync();
            }

            var collection =  _context.Authors as IQueryable<Author>;

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.MainCategory))
            {
                var mainCategory = authorsResourceParameters.MainCategory.Trim();

                collection = collection.Where(a => a.MainCategory == mainCategory);
                //return _context.Authors.Where(a => a.MainCategory == mainCategory);

            }

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.SearchQuery))
            {
               var searchQuery = authorsResourceParameters.SearchQuery.Trim();

                collection = collection.Where(a => a.MainCategory.Contains(searchQuery)
                //return _context.Authors.Where(a => a.MainCategory.Contains(searchQuery)
                   || a.FirstName.Contains(searchQuery)
                   || a.LastName.Contains(searchQuery));
              
            }
            //return await _context.Authors.ToListAsync();

            return collection.ToList();
        }

        public async Task<Author> GetAuthorAsync(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentException(nameof(authorId));
            }

            return await _context.Authors
                .FirstOrDefaultAsync(a => a.Id == authorId);
        }

        //Collections of Authors for author collections
        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            if (authorIds == null)
            {
                throw new ArgumentNullException(nameof(authorIds));
            }

            return _context.Authors.Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .OrderBy(a => a.LastName)
                .ToList();
        }

        public void UpdateAuthor(Author author)
        {
            // no code in this implementation
        }

        public async Task<bool> SaveChangesAsync()
        {
            //return true if 1 or more entities were chnages
            return (await _context.SaveChangesAsync() > 0);
        }

        //Add Author
        public void AddAuthor(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            // the repository fills the id (instead of using identity columns)
            author.Id = Guid.NewGuid();

            foreach (var book in author.Books)
            {
                book.Id = Guid.NewGuid();
            }

            _context.Authors.Add(author);
        }


        public void DeleteAuthor(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            _context.Authors.Remove(author);
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
