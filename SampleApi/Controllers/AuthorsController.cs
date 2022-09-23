using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SampleApi.ActionConstraints;
using SampleApi.Entities;
using SampleApi.Helpers;
using SampleApi.Models;
using SampleApi.ResourceParameters;
using SampleApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleApi.Controllers
{
    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorsRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public AuthorsController(
           IAuthorRepository authorsRepository,
           IMapper mapper, IPropertyMappingService propertyMappingService,
           IPropertyCheckerService propertyCheckerService)
        {
            _authorsRepository = authorsRepository ??
                throw new ArgumentNullException(nameof(authorsRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService??
                throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ??
                throw new ArgumentNullException(nameof(propertyCheckerService));

        }

        [HttpGet(Name ="GetAuthors")]
        public IActionResult GetAuthors(
            [FromQuery]AuthorsResourceParameters authorsResourceParameters)
        {

            //Validate data shaping
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Entities.Author>
               (authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            //Validate Sorting
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }


            var authorsFromRepo =  _authorsRepository.GetAuthors(authorsResourceParameters);

            //Pagination
            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPage = authorsFromRepo.TotalPages
            };

            Response.Headers.Add("X-Pagination",
            JsonSerializer.Serialize(paginationMetadata));

            var links = CreateLinksForAuthors(authorsResourceParameters,
                authorsFromRepo.HasNext,
                authorsFromRepo.HasPrevious);

            var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
                .ShapeData(authorsResourceParameters.Fields);

            var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
            {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"], null);
                authorAsDictionary.Add("links", authorLinks);
                return authorAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links
            };

            return Ok(linkedCollectionResource);
        }

        [Produces("application/json",
            "application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.author.full+json",
            "application/vnd.marvin.author.full+hateoas+json",
            "application/vnd.marvin.author.friendly+json",
            "application/vnd.marvin.author.friendly.hateoas+json")]
        [HttpGet("{authorId}", Name ="GetAuthor")]
        public async Task<ActionResult<Author>> GetAuthor(Guid authorId, string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            //Guard clause against the header(Adevance content negoatation)
            if (!MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            //vALidating the data shaping
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            //Contract btw the client and server with vendor-specific media type
            //check if the link should be added

            var includeLinks = parsedMediaType.SubTypeWithoutSuffix
                .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            IEnumerable<LinkDto> links = new List<LinkDto>();

            if (includeLinks)
            {
                links = CreateLinksForAuthor(authorId, fields);
            }

            //check by string Manipulation
            var primaryMediaType = includeLinks ?
                parsedMediaType.SubTypeWithoutSuffix
                .Substring(0, parsedMediaType.SubTypeWithoutSuffix.Length - 8)
                : parsedMediaType.SubTypeWithoutSuffix;

            //full Author
            if (primaryMediaType == "vnd.marvin.author.full")
            {
                var fullResourceToReturn = _mapper.Map<AuthorFullDto>(authorFromRepo)
                    .ShapeData(fields) as IDictionary<string, object>;

                if (includeLinks)
                {
                    fullResourceToReturn.Add("links", links);
                }
                return Ok(fullResourceToReturn);
            }

            //Friendly author
            var friendlyResourceToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
                .ShapeData(fields) as IDictionary<string, object>;

            if (includeLinks)
            {
                friendlyResourceToReturn.Add("links", links);
            }
            return Ok(friendlyResourceToReturn);

            
        }

        //Author with date of death.
        [HttpPost(Name = "CreateAuthorWithDateOfDeath")]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/vnd.marvin.authorforcreationwithdateofdeath+json")]
        
        public async Task<ActionResult<AuthorDto>> CreateAuthorWithDateOfDeath(
             AuthorForCreationWithDateOfDeathDto author)
        {
            var authorEntity = _mapper.Map<Entities.Author>(author);
            _authorsRepository.AddAuthor(authorEntity);
            await _authorsRepository.SaveChangesAsync();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            //Adding HATEoAS Links
            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null)
                as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);


            return CreatedAtRoute("GetAuthor",
                new { authorId = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }

        //Create Author without Date of date
        [HttpPost(Name = "CreateAuthor")]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/json",
            "application/vnd.marvin.authorforcreation+json")]
        [Consumes("application/json",
            "application/vnd.marvin.authorforcreation+json")]
        public async Task<ActionResult<AuthorDto>> CreateAuthor(
           AuthorForCreationDto author )
        {
            var authorEntity = _mapper.Map<Entities.Author>(author);
            _authorsRepository.AddAuthor(authorEntity);
           await  _authorsRepository.SaveChangesAsync();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            //Adding HATEoAS Links
            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null)
                as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);


            return CreatedAtRoute("GetAuthor",
                new { authorId = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }

        [HttpDelete("{authorId}", Name ="DeleteAuthor")]
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

        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
             switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize,
                            mainCategory = authorsResourceParameters.MainCategory,
                            searchQuery = authorsResourceParameters.SearchQuery
                        });

                case ResourceUriType.NextPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSize = authorsResourceParameters.PageSize,
                            mainCategory = authorsResourceParameters.MainCategory,
                            searchQuery = authorsResourceParameters.SearchQuery
                        });

                case ResourceUriType.Current:


                default:
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize,
                            mainCategory = authorsResourceParameters.MainCategory,
                            searchQuery = authorsResourceParameters.SearchQuery
                        });
            }
        }

        //Method to Create Link For HATEoAS
        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDto(Url.Link("GetAuthor", new { authorId }),
                    "self",
                   "GET"));
            }
            else
            {
                links.Add(
                    new LinkDto(Url.Link("GetAuthor", new { authorId, fields }),
                    "self",
                    "GET"));
            }
            links.Add(
                new LinkDto(Url.Link("DeleteAuthor", new { authorId }),
                "delete_author",
                "DELETE"));

            links.Add(
                new LinkDto(Url.Link("CreateBookForAuthor", new { authorId }),
                "create_book_for_author",
                "POST"));

            links.Add(
                new LinkDto(Url.Link("GetBooksForAuthor", new { authorId }),
                "books",
                "GET"));

            return links;
        }

        //Create Links for collections of Authors
        private IEnumerable<LinkDto> CreateLinksForAuthors(
            AuthorsResourceParameters authorsResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            //self
            links.Add(
                new LinkDto(CreateAuthorsResourceUri // using the Pagination to drive HATEOAS
                (authorsResourceParameters, ResourceUriType.Current)
                , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                    new LinkDto(CreateAuthorsResourceUri(
                        authorsResourceParameters, ResourceUriType.NextPage),
                        "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateAuthorsResourceUri(
                        authorsResourceParameters, ResourceUriType.PreviousPage),
                        "previousPage", "GET"));

            }

            return links;
        }

    }
}
