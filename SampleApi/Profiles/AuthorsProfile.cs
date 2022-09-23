using AutoMapper;
using SampleApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Profiles
{
    public class AuthorsProfile : Profile
    {
        public AuthorsProfile()
        {
            CreateMap<Entities.Author, Models.AuthorDto>()
                .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(
                dest => dest.Age,
                opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge(src.DateOfDeath)));

            //Without date of date
            CreateMap<Models.AuthorForCreationDto, Entities.Author>();

            //For Date of Death
            CreateMap<Models.AuthorForCreationWithDateOfDeathDto, Entities.Author>();

            CreateMap<Entities.Author, Models.AuthorFullDto>();

            
        }
    }
}
