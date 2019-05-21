using System.Collections.Generic;

namespace Films.Core
{
    public class Film
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public int DirectorId { get; set; }
        public Director Director { get; set; }
        public ICollection<ActorFilm> Cast { get; set; }
    }
}
