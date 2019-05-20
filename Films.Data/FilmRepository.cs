using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Films.Core;
using Microsoft.EntityFrameworkCore;

namespace Films.Data
{
    public class FilmRepository : IFilmRepository
    {
        private readonly FilmContext _filmContext;

        public FilmRepository(FilmContext filmContext)
        {
            _filmContext = filmContext;
        }

        public void Add<T>(T entity) where T : class
        {
            _filmContext.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _filmContext.Remove(entity);
        }

        public Task<Actor> GetActorByNameAsync(string actorsName)
        {
            var names = actorsName.Split(" ")[0];
            var query = _filmContext.Actors.Where(a => (a.FirstName + a.LastName).Contains(names[0]) || (a.FirstName + a.LastName).Contains(names[1]));
            return query.FirstOrDefaultAsync();
        }

        public Task<Actor[]> GetAllActorsByFilmAsync(int filmId)
        {
            var query = _filmContext.Actors.Where(a => a.ActorFilms.Contains(new ActorFilm { FilmId = filmId, ActorId = a.Id }));
            return query.ToArrayAsync();
        }

        public Task<Film[]> GetAllFilmsAsync(bool includeCast = false, bool includeDirector = false)
        {
            IQueryable<Film> query;
            if (includeDirector && includeDirector) query = _filmContext.Films.Include(f => f.Director).Include(f => f.Cast);
            else if (includeDirector) query = _filmContext.Films.Include(f => f.Director);
            else if (includeCast) query = _filmContext.Films.Include(f => f.Cast);
            else query = _filmContext.Films;
            return query.ToArrayAsync();
        }

        public Task<Film[]> GetAllFilmsByActorAsync(int actorId, bool includeCast = false, bool includeDirector = false)
        {
            IQueryable<Film> query;
            if (includeDirector && includeDirector) query = _filmContext.Films.Where(f => f.Cast.Contains(new ActorFilm() { FilmId = f.Id, ActorId = actorId })).Include(f => f.Director).Include(f => f.Cast);
            else if (includeDirector) query = _filmContext.Films.Where(f => f.Cast.Contains(new ActorFilm() { FilmId = f.Id, ActorId = actorId })).Include(f => f.Director);
            else if (includeCast) query = _filmContext.Films.Where(f => f.Cast.Contains(new ActorFilm() { FilmId = f.Id, ActorId = actorId })).Include(f => f.Cast);
            else query = _filmContext.Films.Where(f => f.Cast.Contains(new ActorFilm() { FilmId = f.Id, ActorId = actorId }));
            return query.ToArrayAsync();
        }

        public Task<Film[]> GetAllFilmsByDirectorAsync(int directorId, bool includeCast = false, bool includeDirector = false)
        {
            IQueryable<Film> query;
            if (includeDirector && includeDirector) query = _filmContext.Films.Where(f => f.DirectorId == directorId).Include(f => f.Director).Include(f => f.Cast);
            else if (includeDirector) query = _filmContext.Films.Where(f => f.DirectorId == directorId).Include(f => f.Director).Include(f => f.Director);
            else if (includeCast) query = _filmContext.Films.Where(f => f.DirectorId == directorId).Include(f => f.Director).Include(f => f.Cast);
            else query = _filmContext.Films.Where(f => f.DirectorId == directorId);
            return query.ToArrayAsync();
        }

        public Task<Director> GetDirectorByNameAsync(string directorsName)
        {
            var names = directorsName.Split(" ");
            var query = _filmContext.Directors.Where(d => (d.FirstName + d.LastName).Contains(names[0]) && (d.FirstName + d.LastName).Contains(names[1]));
            return query.SingleOrDefaultAsync();
        }

        public Task<Film> GetFilmByTitleAsync(string filmTitle)
        {
            var query = _filmContext.Films.Where(f => f.Title == filmTitle);
            return query.SingleOrDefaultAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _filmContext.SaveChangesAsync()) > 0;
        }

    }
}
