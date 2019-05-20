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

        Task<Actor> GetActorByNameAsync(string actorsName);
        Task<Actor[]> GetAllActorsByFilmAsync(int filmId);
        Task<Film[]> GetAllFilmsAsync(bool includeActors = false, bool includeDirector = false);
        Task<Film[]> GetAllFilmsByActorAsync(int actorId, bool includeActors = false, bool includeDirector = false);
        Task<Film[]> GetAllFilmsByDirectorAsync(int directorId, bool includeActors = false, bool includeDirector = false);
        Task<Director> GetDirectorByNameAsync(string directorsName);
        Task<Film> GetFilmByTitleAsync(string filmTitle);

    }
}
