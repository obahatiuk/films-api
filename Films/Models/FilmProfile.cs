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
                .ForMember(dto => dto.Films, opt => opt.MapFrom(a => a.ActorFilms))
                .ReverseMap();

            CreateMap<Director, DirectorModel>()
                .ReverseMap();

            CreateMap<FilmModel, ActorFilm>()
                .ForPath(dto => dto.Film, opt => opt.MapFrom(a => a))
                .ReverseMap();

            CreateMap<ActorModel, ActorFilm>()
                .ForPath(dto => dto.Actor, opt => opt.MapFrom(a => a))
                .ReverseMap();

            CreateMap<FilmModel, Film>()
                .ForMember(dto => dto.Id, opt => opt.Ignore())
                .ForMember(dto => dto.DirectorId, opt => opt.Ignore())
            .ReverseMap();
        }
    }
}
