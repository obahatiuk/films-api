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
            CreateMap<Film, FilmModel>()
                .ForMember(dto => dto.Cast, opt => opt.MapFrom(f => f.Cast));
            CreateMap<Actor, ActorModel>();
            CreateMap<Director, DirectorModel>();
        }
    }
}
