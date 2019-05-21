using Films.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Films.Data
{
    public interface IFilmRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveChangesAsync();

        Task<Actor> GetActorByNameAsync(string actorsFirstName, string actorsLastName);
        Task<Actor[]> GetActorsByFilmAsync(int filmId);
        Task<Actor> GetActorById(int id);

        Task<Film[]> GetAllFilmsAsync(bool includeActors = false, bool includeDirector = false);
        Task<Film[]> GetAllFilmsByActorAsync(int actorId, bool includeCast = false, bool includeDirector = false);
        Task<Film[]> GetAllFilmsByDirectorAsync(int directorId, bool includeCast = false, bool includeDirector = false);
        Task<Film> GetFilmByTitleAsync(string filmTitle, bool includeCast = false, bool includeDirector = false);

        Task<Director> GetDirectorByNameAsync(string directorsFirstName, string directorsLastName);
        Task<Director> GetDirectorById(int id);
    }
}
