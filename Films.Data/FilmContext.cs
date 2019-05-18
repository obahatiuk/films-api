using Films.Core;
using Microsoft.EntityFrameworkCore;
using System;

namespace Films.Data
{
    public class FilmContext : DbContext 
    {
        public DbSet<Film> Films { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Director> Directors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActorFilm>()
                .HasKey(af => new { af.ActorId, af.FilmId });
            base.OnModelCreating(modelBuilder);
        }
    }
}
