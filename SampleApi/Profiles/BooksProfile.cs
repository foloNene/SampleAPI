using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Profiles
{
    public class BooksProfile : Profile
    {
        public BooksProfile()
        {
            CreateMap<Entities.Book, Models.BookDto>();

            CreateMap<Models.BookForCreationDto, Entities.Book>();

            CreateMap<Models.BookForUpdateDto, Entities.Book>();

            CreateMap<Entities.Book, Models.BookForUpdateDto>();
        }
    }
}
