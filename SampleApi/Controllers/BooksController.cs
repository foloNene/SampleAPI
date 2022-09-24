using AutoMapper;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SampleApi.Models;
using SampleApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/books")]

    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    //[ResponseCache(CacheProfileName = "240SecondsCacheProfile")]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        public BooksController(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _mapper = mapper;

        }

        [HttpGet(Name = "GetBooksForAuthor")]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge =1000)]
       // [ResponseCache(Duration = 120)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(
        Guid authorId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _bookRepository.GetBooksAsync(authorId);
            return Ok(_mapper.Map<IEnumerable<BookDto>>(booksFromRepo));
        }

        [HttpGet("{bookId}", Name = "GetBookForAuthor")]
        public async Task<ActionResult<BookDto>> GetBook(
             Guid authorId,
             Guid bookId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<BookDto>(bookFromRepo));
        }

        [HttpPost(Name = "CreateBookForAuthor")]
        public async Task<ActionResult<BookDto>> CreateBookForAuthor(
            Guid authorId, BookForCreationDto book)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookEntity = _mapper.Map<Entities.Book>(book);

            _bookRepository.AddBook(authorId, bookEntity);

            await _bookRepository.SaveChangesAsync();

            var bookToReturn = _mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute(
                "GetBookForAuthor",
                new { authorId = authorId,
                    bookId = bookToReturn.Id },
                        bookToReturn);
        }

        [HttpPut("{bookId}")]
        public async Task <IActionResult> UpdateBookForAuthor(Guid authorId, Guid bookId, BookForUpdateDto book)
        {
            if (!await _bookRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(book, bookForAuthorFromRepo);

            _bookRepository.UpdateBook(bookForAuthorFromRepo);

            await _bookRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{bookId}")]
        public async Task <IActionResult> PartiallyUpdateBookForauthor(Guid authorId, Guid bookId, JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            if (!await _bookRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookToPatch = _mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);
            
            //add validation
            patchDocument.ApplyTo(bookToPatch, ModelState);

            if (!TryValidateModel(bookToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(bookToPatch, bookForAuthorFromRepo);
            _bookRepository.UpdateBook(bookForAuthorFromRepo);

            await _bookRepository.SaveChangesAsync();

            return NoContent();


        }

        
        [HttpDelete("{bookId}")]
        public async Task<IActionResult> DeleteCourseForAuthor(Guid authorId, Guid bookId)
        {
            if (!await _bookRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _bookRepository.DeleteBook(bookForAuthorFromRepo);
            await _bookRepository.SaveChangesAsync();

            return NoContent();
        }


        public override ActionResult ValidationProblem(
            [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }


    }
}
