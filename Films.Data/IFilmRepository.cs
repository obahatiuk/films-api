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

        Task<Actor> GetActorByNameAsync(string actorsFirstName, string actorsLastName, bool includeFilms = false);
        Task<Actor[]> GetAllActorsAsync(bool includeFilms = false);

        Task<Film[]> GetAllFilmsAsync(bool includeActors = false, bool includeDirector = false);
        Task<Film> GetFilmByTitleAsync(string filmTitle, bool includeCast = false, bool includeDirector = false, bool includeActorFilm = false);

        Task<Director> GetDirectorByNameAsync(string directorsFirstName, string directorsLastName, bool includeFilms = false);
        Task<Director[]> GetAllDirectorsAsync(bool includeFilms = false);

    }
}
