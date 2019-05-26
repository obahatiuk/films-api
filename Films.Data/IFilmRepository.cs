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
        void UndoChanges();
        void RollBack();
        Task<bool> SaveChangesAsync();

        Task<Actor> GetActorByNameAsync(string actorsFirstName, string actorsLastName, bool includeCast = false);
        Task<Actor[]> GetActorsByFilmAsync(int filmId);
        Task<Actor> GetActorById(int id);
        Task<Actor[]> GetAllActorsAsync(bool includeFilms = false);

        Task<Film[]> GetAllFilmsAsync(bool includeActors = false, bool includeDirector = false);
        Task<Film[]> GetAllFilmsByActorAsync(int actorId, bool includeCast = false, bool includeDirector = false);
        Task<Film[]> GetAllFilmsByDirectorAsync(int directorId, bool includeCast = false, bool includeDirector = false);
        Task<Film> GetFilmByTitleAsync(string filmTitle, bool includeCast = false, bool includeDirector = false);

        Task<Director> GetDirectorByNameAsync(string directorsFirstName, string directorsLastName, bool includeFilms = false);
        Task<Director> GetDirectorById(int id);
        Task<Director[]> GetAllDirectorsAsync(bool includeFilms = false);

    }
}
