using SampleApi.Entities;
using SampleApi.ResourceParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Services
{
    public interface IAuthorRepository : IDisposable
    {
        Task<bool> AuthorExistsAsync(Guid authorId);

        Task<IEnumerable<Author>> GetAuthorsAsync();
        //For authorcollection
        IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);

        Task<IEnumerable<Author>> GetAuthorsAsync(AuthorsResourceParameters authorsResourceParameters);

        Task<Author> GetAuthorAsync(Guid authorId);

        void UpdateAuthor(Author author);

        Task<bool> SaveChangesAsync();
        //Add Authors
        void AddAuthor(Author author);

        void DeleteAuthor(Author author);
    }
}
