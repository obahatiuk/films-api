﻿using Films.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Films.Data
{
    public class FilmContext : DbContext
    {
        public DbSet<Core.Film> Films { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Director> Directors { get; set; }

        public FilmContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActorFilm>()
                .HasKey(af => new { af.ActorId, af.FilmId });

            base.OnModelCreating(modelBuilder);
        }
    }
}
