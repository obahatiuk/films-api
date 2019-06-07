using Films.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Films.Data
{
    public class FakeFilmRepository : IFilmRepository
    {
        private List<Director> Directors { get; set; }
        private ICollection<Actor> Actors { get; set; }
        private List<Film> Films { get; set; }

        private List<Director> DirectorsToAdd { get; set; }
        private List<Actor> ActorsToAdd { get; set; }
        private List<Film> FilmsToAdd { get; set; }

        private List<Director> DirectorsToDelete { get; set; }
        private List<Actor> ActorsToDelete { get; set; }
        private List<Film> FilmsToDelete { get; set; }

        public FakeFilmRepository()
        {
            Directors = new List<Director>();
            Actors = new List<Actor>();
            Films = new List<Film>();
            Initialize();
        }

        private void Initialize()
        {

            DirectorsToAdd = new List<Director>();
            ActorsToAdd = new List<Actor>();
            FilmsToAdd = new List<Film>();

            DirectorsToDelete = new List<Director>();
            ActorsToDelete = new List<Actor>();
            FilmsToDelete = new List<Film>();
        }

        public void Add<T>(T entity) where T : class
        {
            switch (entity.GetType().ToString())
            {
                case ("Films.Core.Director"):
                    DirectorsToAdd.Add(entity as Director);
                    break;
                case ("Films.Core.Film"):
                    FilmsToAdd.Add(entity as Film);
                    break;
                case ("Films.Core.Actor"):
                    ActorsToAdd.Add(entity as Actor);
                    break;
                default:
                    break;
            }
        }

        public void Delete<T>(T entity) where T : class
        {
            switch (entity.GetType().ToString())
            {
                case ("Films.Core.Director"):
                    DirectorsToDelete.Add(entity as Director);
                    break;
                case ("Films.Core.Film"):
                    FilmsToDelete.Add(entity as Film);
                    break;
                case ("Films.Core.Actor"):
                    ActorsToDelete.Add(entity as Actor);
                    break;
                default:
                    break;
            }
        }

        public Task<Actor> GetActorByNameAsync(string actorsFirstName, string actorsLastName, bool includeFilms = false)
        {
            return Task.Run(() => {
                if (includeFilms) return Actors.Where(a => a.FirstName == actorsFirstName && a.LastName == actorsFirstName).Select(a => new Actor()
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    ActorFilms = a.ActorFilms.Select(af => new ActorFilm() { Film = Films.Where(f => f.Id == af.FilmId).SingleOrDefault() }).ToList()
                }).SingleOrDefault();
                else return Actors.Where(a => a.FirstName == actorsFirstName && a.LastName == actorsFirstName).SingleOrDefault();
            });
        }

        public Task<Actor[]> GetAllActorsAsync(bool includeFilms = false)
        {
            return Task.Run(() => {
                if (includeFilms) return Actors.Select(a => new Actor()
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    ActorFilms = a.ActorFilms.Select(af => new ActorFilm() { Film = Films.Where(f => f.Id == af.FilmId).SingleOrDefault() }).ToList()
                }).ToArray();
                else return Actors.ToArray();
            });
        }

        public Task<Director[]> GetAllDirectorsAsync(bool includeFilms = false)
        {
            return Task.Run(() => {
                var result = Directors.ToArray();
                if (includeFilms)
                {
                    foreach (var director in result)
                    {
                        director.Films = Films.Where(f => f.DirectorId == director.Id).ToList();
                    }
                }
                return result;
            });
        }

        public Task<Film[]> GetAllFilmsAsync(bool includeActors = false, bool includeDirector = false)
        {
            return Task.Run(() =>
            {
                var result = Films.ToArray();
                if (includeActors && result != null && result.Count() > 0)
                {
                    foreach (var film in result)
                    {
                        if (film.Cast != null)
                        {
                            foreach (var actorFilm in film.Cast)
                            {
                                actorFilm.Actor = Actors.Where(a => a.Id == actorFilm.ActorId).SingleOrDefault();
                            }
                        }

                        if (includeDirector && result != null)
                        {
                            film.Director = Directors.Where(d => d.Id == film.DirectorId).SingleOrDefault();
                        }
                    }
                }

                return result;
            });
        }

        public Task<Director> GetDirectorByNameAsync(string directorsFirstName, string directorsLastName, bool includeFilms = false)
        {
            return Task.Run(() => {
                var result = Directors.Where(d => d.FirstName == directorsFirstName && d.LastName == directorsLastName).FirstOrDefault();
                if (includeFilms && result != null)
                {
                    result.Films = Films.Where(f => f.DirectorId == result.Id).ToList();
                }
                return result;
            });
        }

        public Task<Film> GetFilmByTitleAsync(string filmTitle, bool includeCast = false, bool includeDirector = false, bool includeActorFilm = false)
        {
            return Task.Run(() =>
            {
                var result = Films.Where(f => f.Title == filmTitle).FirstOrDefault();
                if (includeCast && result != null)
                {
                    if (result.Cast != null)
                    {
                        foreach (var actorFilm in result.Cast)
                        {
                            actorFilm.Actor = Actors.Where(a => a.Id == actorFilm.ActorId).SingleOrDefault();
                        }
                    }
                }

                if (includeDirector && result != null)
                {
                    result.Director = Directors.Where(d => d.Id == result.DirectorId).SingleOrDefault();
                }

                return result;
            });
        }

        public Task<bool> SaveChangesAsync()
        {
            return Task.Run(() =>
            {
                if (FilmsToAdd.Count() <= 0 && ActorsToAdd.Count() <= 0
                && DirectorsToAdd.Count() <= 0 && FilmsToDelete.Count() <= 0
                && ActorsToDelete.Count() <= 0 && DirectorsToDelete.Count() <= 0) return false;

                foreach (var film in FilmsToAdd)
                {
                    film.Id = Films.Count() + 1;
                    Films.Add(film);
                }
                foreach (var actor in ActorsToAdd)
                {
                    actor.Id = Actors.Count() + 1;
                    Actors.Add(actor);
                }
                foreach (var director in DirectorsToAdd)
                {
                    director.Id = Directors.Count() + 1;
                    Directors.Add(director);
                }

                foreach (var film in FilmsToDelete)
                {
                    var filmInList = Films.Where(f => f.Id == film.Id).FirstOrDefault();
                    Films.Remove(filmInList);
                }
                foreach (var actor in ActorsToDelete)
                {
                    var actorInList = Actors.Where(f => f.Id == actor.Id).FirstOrDefault();
                    Actors.Remove(actorInList);
                }
                foreach (var director in DirectorsToDelete)
                {
                    var directorInList = Directors.Where(f => f.Id == director.Id).FirstOrDefault();
                    Directors.Remove(directorInList);
                }

                Initialize();
                return true;
            });
        }
    }
}
