using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Films.Models
{
    public class FilmModel
    {
        [Required]
        public string Title { get; set; }
        public string Overview { get; set; }
        //public int? DirectorId { get; set; }
        public DirectorModel Director { get; set; }
        public IEnumerable<ActorModel> Cast { get; set;}
    }
}
