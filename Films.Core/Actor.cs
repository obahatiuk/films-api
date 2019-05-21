using System;
using System.Collections.Generic;
using System.Text;

namespace Films.Core
{
    public class Actor : Person
    {
        public ICollection<ActorFilm> ActorFilms { get; set; }
    }
}
