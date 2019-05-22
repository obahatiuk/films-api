using AutoMapper;
using Films.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Films.Models
{
    public class FilmProfile : Profile
    {
        public FilmProfile()
        {
            CreateMap<Actor, ActorModel>()
                .ReverseMap();

            CreateMap<Director, DirectorModel>()
                .ReverseMap();

            CreateMap<FilmModel, ActorFilm>()
                .ForMember(dto => dto.Film, opt => opt.MapFrom(a => a))
                .ReverseMap();

            CreateMap<ActorFilm, ActorModel>()
                .ForMember(dto => dto.FirstName, opt => opt.MapFrom(a => a.Actor.FirstName))
                .ForMember(dto => dto.LastName, opt => opt.MapFrom(a => a.Actor.LastName))
                .ReverseMap();

            CreateMap<FilmModel, Film>()
                .ForMember(dto => dto.Id, opt => opt.Ignore())
                .ForMember(dto => dto.DirectorId, opt => opt.Ignore())
            .ReverseMap();
        }
    }
}
