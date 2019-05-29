using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Films.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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

        public Task<Actor> GetActorById(int id)
        {
            var query = _filmContext.Actors.Where(a => a.Id == id);
            return query.SingleOrDefaultAsync();
        }

        public Task<Director> GetDirectorById(int id)
        {
            var query = _filmContext.Directors.Where(d => d.Id == id);
            return query.SingleOrDefaultAsync();
        }

        public Task<Actor[]> GetAllActorsAsync(bool includeFilms = false)
        {
            if (includeFilms) return _filmContext.Actors.Include(d => d.ActorFilms).ThenInclude(af => af.Film).ToArrayAsync();
            else return _filmContext.Actors.ToArrayAsync();
        }

        public Task<Actor> GetActorByNameAsync(string actorsFirstName, string actorsLastName, bool includeCast = false)
        {
            IQueryable<Actor> query;
            if (includeCast) query = _filmContext.Actors.Where(a => (a.FirstName == actorsFirstName && a.LastName == actorsLastName)).Include(a => a.ActorFilms).ThenInclude(af => af.Film);
            else query = _filmContext.Actors.Where(a => (a.FirstName == actorsFirstName && a.LastName == actorsLastName)).Include(a => a.ActorFilms);
            return query.FirstOrDefaultAsync();
        }

        public Task<Actor[]> GetActorsByFilmAsync(int filmId)
        {
            var query = _filmContext.Actors.Where(a => a.ActorFilms.Where(af => af.FilmId == filmId && af.ActorId == a.Id ) != null);
            return query.ToArrayAsync();
        }

        public Task<Film[]> GetAllFilmsAsync(bool includeCast = false, bool includeDirector = false)
        {
            IQueryable<Film> query;
            if (includeDirector && includeDirector) query = _filmContext.Films.Select(f => new Film()
            {
                Id = f.Id,
                Title = f.Title,
                Cast = f.Cast.Select( c => new ActorFilm() { Actor = new Actor() { Id = c.Actor.Id, FirstName = c.Actor.FirstName, LastName = c.Actor.LastName}}).ToList(),
                Director = new Director() { FirstName = f.Director.FirstName, LastName = f.Director.LastName, Id = f.Director.Id }
            });
            else if (includeDirector) query = _filmContext.Films.Include(f => f.Director);
            else if (includeCast) query = _filmContext.Films.Select(f => new Film()
            {
                Id = f.Id,
                Title = f.Title,
                Cast = f.Cast.Select(c => new ActorFilm() { Actor = new Actor() { Id = c.Actor.Id, FirstName = c.Actor.FirstName, LastName = c.Actor.LastName } }).ToList(),
            });
            else query = _filmContext.Films;
            return query.ToArrayAsync();
        }

        public Task<Film[]> GetAllFilmsByActorAsync(int actorId, bool includeCast = false, bool includeDirector = false)
        {
            IQueryable<Film> query;
            if (includeDirector && includeDirector) query = _filmContext.Films.Where(f => f.Cast.Where(af => af.FilmId == f.Id && af.ActorId == actorId) != null).Include(f => f.Director).Include(f => f.Cast);
            else if (includeDirector) query = _filmContext.Films.Where(f => f.Cast.Where(af => af.FilmId == f.Id && af.ActorId == actorId) != null).Include(f => f.Director);
            else if (includeCast) query = _filmContext.Films.Where(f => f.Cast.Where(af => af.FilmId == f.Id && af.ActorId == actorId) != null).Include(f => f.Cast);
            else query = _filmContext.Films.Where(f => f.Cast.Where(af => af.FilmId == f.Id && af.ActorId == actorId) != null);
            return query.ToArrayAsync();
        }

        public Task<Film[]> GetAllFilmsByDirectorAsync(int directorId, bool includeCast = false, bool includeDirector = false)
        {
            IQueryable<Film> query;
            if (includeDirector && includeDirector) query = _filmContext.Films.Where(f => f.DirectorId == directorId).Include(f => f.Director).Include(f => f.Cast);//.Select(c => new { c.Cast..Actor.FirstName, c.Actor.LastName });
            else if (includeDirector) query = _filmContext.Films.Where(f => f.DirectorId == directorId).Include(f => f.Director).Include(f => f.Director);
            else if (includeCast) query = _filmContext.Films.Where(f => f.DirectorId == directorId).Include(f => f.Cast);
            else query = _filmContext.Films.Where(f => f.DirectorId == directorId);
            return query.ToArrayAsync();
        }

        public Task<Director[]> GetAllDirectorsAsync(bool includeFilms = false)
        {
            if (includeFilms) return _filmContext.Directors.Include(d => d.Films).ToArrayAsync();
            else return _filmContext.Directors.ToArrayAsync();
        }

        public Task<Director> GetDirectorByNameAsync(string directorsFirstName, string directorsLastName, bool includeFilms = false)
        {
            if (includeFilms) return _filmContext.Directors.Where(d => d.FirstName == directorsFirstName && d.LastName == directorsLastName).Include(d => d.Films).SingleOrDefaultAsync();
            else return _filmContext.Directors.Where(d => d.FirstName == directorsFirstName && d.LastName == directorsLastName).SingleOrDefaultAsync();
        }

        public Task<Film> GetFilmByTitleAsync(string filmTitle, bool includeCast = false, bool includeDirector = false)
        {
            IQueryable<Film> query;
            if (includeDirector && includeDirector) query = _filmContext.Films.Where(f => f.Title == filmTitle).Include(f => f.Director).Include(f => f.Cast);
            else if (includeDirector) query = _filmContext.Films.Where(f => f.Title == filmTitle).Include(f => f.Director).Include(f => f.Director);
            else if (includeCast) query = _filmContext.Films.Where(f => f.Title == filmTitle).Include(f => f.Director).Include(f => f.Cast);
            else query = _filmContext.Films.Where(f => f.Title == filmTitle);
            return query.SingleOrDefaultAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _filmContext.SaveChangesAsync()) > 0;
        }

        public void UndoChanges()
        {
            foreach (var entry in _filmContext.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
