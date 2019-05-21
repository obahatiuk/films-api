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
            CreateMap<Actor, ActorModel>().ReverseMap();

            CreateMap<ActorFilm, ActorModel>()
                .ForMember(dto => dto.FirstName, opt => opt.MapFrom(a => a.Actor.FirstName))
                .ForMember(dto => dto.LastName, opt => opt.MapFrom(a => a.Actor.LastName))
                .ReverseMap();
            CreateMap<Film, FilmModel>()
                .ForMember(dto => dto.Cast, opt => opt.MapFrom(f => f.Cast));
            CreateMap<Director, DirectorModel>();
        }
    }
}
