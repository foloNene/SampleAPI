using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Entities;
using SampleApi.Models;
using SampleApi.ResourceParameters;
using SampleApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Controllers
{
    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorsRepository;
        private readonly IMapper _mapper;

        public AuthorsController(
           IAuthorRepository authorsRepository,
           IMapper mapper)
        {
            _authorsRepository = authorsRepository ??
                throw new ArgumentNullException(nameof(authorsRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
            [FromQuery]AuthorsResourceParameters authorsResourceParameters)
        {
            var authorsFromRepo = await _authorsRepository.GetAuthorsAsync(authorsResourceParameters);
            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
        }

        [HttpGet("{authorId}", Name ="GetAuthor")]
        public async Task<ActionResult<Author>> GetAuthor(Guid authorId)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
        }

        [HttpPost()]
        public async Task<ActionResult<AuthorDto>> CreateAuthor(
           AuthorForCreationDto author )
        {
            var authorEntity = _mapper.Map<Entities.Author>(author);
            _authorsRepository.AddAuthor(authorEntity);
           await  _authorsRepository.SaveChangesAsync();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor",
                new { authorId = authorToReturn.Id },
                authorToReturn);
        }

        [HttpDelete("{authorId}")]
        public async Task<ActionResult> DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);

            if(authorFromRepo == null)
            {
                return NotFound(); 
            }

            _authorsRepository.DeleteAuthor(authorFromRepo);
            await _authorsRepository.SaveChangesAsync();

            return NoContent();
        }

    }
}
