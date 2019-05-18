using System;
using System.Collections.Generic;
using System.Text;

namespace Films.Core
{
    public class Director : Person
    {
        public ICollection<Film> Films { get; set; }
    }
}
