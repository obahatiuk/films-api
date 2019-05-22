using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Films.Models
{
    public abstract class FilmIndustryEmployee : PersonModel
    {
        public ICollection<FilmModel> Films { get; set; }
    }
}
