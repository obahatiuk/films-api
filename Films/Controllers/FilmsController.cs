using AutoMapper;
using Films.Core;
using Films.Data;
using Films.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Films.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilmsController : ControllerBase
    {
        private readonly IFilmRepository _repository;
        private readonly IMapper _mapper;

        public FilmsController(IFilmRepository repository, IMapper mapper) {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<FilmModel[]>> Get(bool includeCast = false, bool includeDirector = false)
        {
            try
            {
                var results = await _repository.GetAllFilmsAsync(includeCast, includeDirector);

                return _mapper.Map<FilmModel[]>(results);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<FilmModel>> Post(FilmModel model)
        {
            try
            {
                if (model.Cast != null && model.Cast.Count() > 0) return BadRequest("Please add film first. Update films cast with updatefilm method");

                var directorModelValidationResult = ModelsValidator.ValidateDirectorModel(model);

                if (!directorModelValidationResult.Item1) return BadRequest(directorModelValidationResult.Item2);

                var actorsModelValidationResult = ModelsValidator.ValidateActorModels(model);

                if (!actorsModelValidationResult.Item1) return BadRequest(actorsModelValidationResult.Item2);

                var film = await _repository.GetFilmByTitleAsync(model.Title);
                if (film != null) return BadRequest("There is a film with same name");

                film = _mapper.Map<Film>(model);

                var updateDirectorResult = await UpdateDirectorAsync(film);

                if (!updateDirectorResult.Item1) return BadRequest(updateDirectorResult.Item2);

                var updateCastResult = await UpdateCastAsync(film);

                if (!updateCastResult.Item1) return BadRequest(updateCastResult.Item2);

                _repository.Add(film);

                if (await _repository.SaveChangesAsync())
                {
                    film = await _repository.GetFilmByTitleAsync(model.Title, true, true);
                    return Created($"/api/films/{model.Title}", _mapper.Map<FilmModel>(film));
                }
            
                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut("updatefilm/{filmName}")]
        public async Task<ActionResult<FilmModel>> UpdateFilm(string filmName, FilmModel model)
        {
            try
            {
                var directorModelValidationResult = ModelsValidator.ValidateDirectorModel(model);

                if (!directorModelValidationResult.Item1) return BadRequest(directorModelValidationResult.Item2);

                var actorsModelValidationResult = ModelsValidator.ValidateActorModels(model);

                if (!actorsModelValidationResult.Item1) return BadRequest(actorsModelValidationResult.Item2);

                var film = await _repository.GetFilmByTitleAsync(filmName);
                if (film == null) return BadRequest("Film doesn't exist in db please add film with post method /api/films");

                _mapper.Map(model, film);

                var updateDirectorResult = await UpdateDirectorAsync(film);

                if (!updateDirectorResult.Item1) return BadRequest(updateDirectorResult.Item2);

                var updateCastResult = await UpdateCastAsync(film);

                if (!updateCastResult.Item1) return BadRequest(updateCastResult.Item2);

                if (await _repository.SaveChangesAsync())
                {
                    film = await _repository.GetFilmByTitleAsync(film.Title, true, true);
                    return _mapper.Map<FilmModel>(film);

                }
                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }


        [HttpDelete("{filmName}")]
        public async Task<ActionResult> DeleteFilm(string filmName)
        {
            try
            {
                var film = await _repository.GetFilmByTitleAsync(filmName);
                if (film == null) return BadRequest("Film doesn't exist");

                _repository.Delete(film);

                if (await _repository.SaveChangesAsync()) return Ok($"Film {filmName} was deleted");

                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut("addToCast/{filmName}")]
        public async Task<ActionResult> AddActorToCast(string filmName, ActorModel model)
        {
            try
            {
                if (model.Films != null && model.Films.Count() > 0) return BadRequest("Actor should not contain films");

                var film = await _repository.GetFilmByTitleAsync(filmName, true);
                if (film == null) return BadRequest("Film doesn't exist in db please add film with post method /api/films");

                var actor = await _repository.GetActorByNameAsync(model.FirstName, model.LastName);
                if (actor == null) return BadRequest("Actor doesn't exist in db please add actor with post method /api/actors");

                if (film.Cast.Where(af => af.ActorId == actor.Id && af.FilmId == film.Id).FirstOrDefault() != null) return BadRequest($"Actor is already mentioned in cast for {filmName}");

                _repository.Add(new ActorFilm() { ActorId = actor.Id, FilmId = film.Id });

                if (await _repository.SaveChangesAsync()) return Ok("Actor added");

                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpDelete("deleteFromCast/{filmName}")]
        public async Task<ActionResult> DeleteActorFromCast(string filmName, ActorModel model)
        {
            try
            {
                var film = await _repository.GetFilmByTitleAsync(filmName, true);
                if (film == null) return BadRequest("Film doesn't exist in db please add film with post method /api/films");

                var actor = await _repository.GetActorByNameAsync(model.FirstName, model.LastName);
                if (actor == null) return BadRequest("Actor doesn't exist in db please add actor with post method /api/actors");

                var castEntity = film.Cast.Where(af => af.ActorId == actor.Id && af.FilmId == film.Id).FirstOrDefault();

                if (castEntity == null) return BadRequest($"The actor isn't mentioned in cast for {filmName}");

                _repository.Delete(castEntity);

                if (await _repository.SaveChangesAsync()) return Ok("Actor Deleted");

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        private async Task<Tuple<bool, string>> UpdateDirectorAsync(Film film)
        {
            var director = await _repository.GetDirectorByNameAsync(film.Director.FirstName, film.Director.LastName);

            if (director == null) return Tuple.Create(false, "Director not found. Please add director first");

            film.Director.Id = director.Id;
            film.DirectorId = director.Id;
            
            return Tuple.Create(true, "");
        }

        private async Task<Tuple<bool, string>> UpdateCastAsync(Film film)
        {
            if (film.Cast.Count() > 0)
            {
                var actors = film.Cast.Select(af => af.Actor).ToArray();

                film.Cast = new List<ActorFilm>();

                for(int i = 0; i < actors.Count(); i++)
                {
                    if(string.IsNullOrEmpty(actors.ElementAt(i).FirstName) || string.IsNullOrEmpty(actors.ElementAt(i).LastName)) return Tuple.Create(false, $"{actors.ElementAt(i).FirstName} {actors.ElementAt(i).LastName} name is invalid.");

                    var actorDBEntity = await _repository.GetActorByNameAsync(actors.ElementAt(i).FirstName, actors.ElementAt(i).LastName, true);

                    if (actorDBEntity == null) return Tuple.Create(false, $"Actor {actors.ElementAt(i).FirstName} {actors.ElementAt(i).LastName} not found. Please add actor first");

                    var isThereActorInCast = actorDBEntity.ActorFilms != null && actorDBEntity.ActorFilms.Where(c => c.FilmId == film.Id).Any();

                    if (!isThereActorInCast) film.Cast.Add(new ActorFilm() { ActorId = actorDBEntity.Id, FilmId = film.Id});
                }
            }

            return Tuple.Create(true, "");
        }
    }
}
