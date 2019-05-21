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

        public Task<Actor> GetActorByNameAsync(string actorsFirstName, string actorsLastName)
        {
            var query = _filmContext.Actors.Where(a => (a.FirstName == actorsFirstName && a.LastName == actorsLastName));
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
            if (includeDirector && includeDirector) query = _filmContext.Films.Include(f => f.Director).Include(f => f.Cast).ThenInclude(a => a.Actor);
            else if (includeDirector) query = _filmContext.Films.Include(f => f.Director);
            else if (includeCast) query = _filmContext.Films.Include(f => f.Cast);
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
            if (includeDirector && includeDirector) query = _filmContext.Films.Where(f => f.DirectorId == directorId).Include(f => f.Director).Include(f => f.Cast);
            else if (includeDirector) query = _filmContext.Films.Where(f => f.DirectorId == directorId).Include(f => f.Director).Include(f => f.Director);
            else if (includeCast) query = _filmContext.Films.Where(f => f.DirectorId == directorId).Include(f => f.Director).Include(f => f.Cast);
            else query = _filmContext.Films.Where(f => f.DirectorId == directorId);
            return query.ToArrayAsync();
        }

        public Task<Director> GetDirectorByNameAsync(string directorsFirstName, string directorsLastName)
        {
            var query = _filmContext.Directors.Where(d => d.FirstName ==  directorsFirstName && d.LastName == directorsLastName);
            return query.SingleOrDefaultAsync();
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

    }
}
