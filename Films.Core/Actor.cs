using System;
using System.Collections.Generic;
using System.Text;

namespace Films.Core
{
    public class Actor : Person
    {
        public List<ActorFilm> ActorFilms { get; set; }
    }
}
