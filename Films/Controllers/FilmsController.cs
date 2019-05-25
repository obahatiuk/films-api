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
                var film = await _repository.GetFilmByTitleAsync(model.Title);
                if (film != null) return BadRequest("There is a film with same name");
                if (model.Director.FirstName == null || model.Director.LastName == null) return BadRequest("There is no director provided");

                film = _mapper.Map<Film>(model);

                var updateDirectorResult = await UpdateDirector(film);

                if (!updateDirectorResult.Item1) return BadRequest(updateDirectorResult.Item2);

                var updateCastResult = await UpdateDirector(film);

                if (!updateCastResult.Item1) return BadRequest(updateCastResult.Item2);

                _repository.Add(film);

                if (await _repository.SaveChangesAsync()) return Created($"/api/films/{model.Title}", _mapper.Map<FilmModel>(film));
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

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
                var film = await _repository.GetFilmByTitleAsync(filmName);
                if (film == null) return BadRequest("Film doesn't exist in db please add film with post method /api/films");

                _mapper.Map(model, film);

                var updateDirectorResult = await UpdateDirector(film);

                if (!updateDirectorResult.Item1) return BadRequest(updateDirectorResult.Item2);

                var updateCastResult = await UpdateDirector(film);

                if (!updateCastResult.Item1) return BadRequest(updateCastResult.Item2);

                if (await _repository.SaveChangesAsync())
                {
                    film = await _repository.GetFilmByTitleAsync(film.Title);
                    return _mapper.Map<FilmModel>(film);

                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }


        [HttpDelete("{filmname}")]
        public async Task<ActionResult> DeleteFilm(string filmName)
        {
            try
            {
                var film = await _repository.GetFilmByTitleAsync(filmName);
                if (film == null) return BadRequest("Film doesn't exist");

                _repository.Delete(film);

                if (await _repository.SaveChangesAsync()) return Ok($"Film {filmName} was deleted");

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
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
                var film = await _repository.GetFilmByTitleAsync(filmName, true);
                if (film == null) return BadRequest("Film doesn't exist in db please add film with post method /api/films");

                var actor = await _repository.GetActorByNameAsync(model.FirstName, model.LastName);
                if (actor == null) return BadRequest("Actor doesn't exist in db please add actor with post method /api/actors");

                if (film.Cast.Where(af =>  af.ActorId == actor.Id && af.FilmId == film.Id).FirstOrDefault() != null) return BadRequest($"Actor is already mentioned in cast for {filmName}");

                _repository.Add(new ActorFilm() { ActorId = actor.Id, FilmId = film.Id});

                if (await _repository.SaveChangesAsync()) return Ok("Actor added");

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

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

        public async Task<Tuple<bool, string>> UpdateDirector(Film film)
        {
            if (film.Director != null)
            {
                if (string.IsNullOrEmpty(film.Director.FirstName) || string.IsNullOrEmpty(film.Director.LastName)) return Tuple.Create(true, "Directors name invalid");
                var director = await _repository.GetDirectorByNameAsync(film.Director.FirstName, film.Director.LastName);
                if (director == null) return Tuple.Create(true, "Director not found. Please add director first");
                film.Director.Id = director.Id;
            }
            return Tuple.Create(true, "");
        }


        private async Task<Tuple<bool, string>> UpdateCastAsync(Film film)
        {
            if (film.Cast.Count() != 0)
            {
                var actors = film.Cast.Select(af => af.Actor).ToArray();
                foreach (var actor in actors)
                {
                    if (string.IsNullOrEmpty(actor.FirstName) || string.IsNullOrEmpty(actor.LastName)) return Tuple.Create(false, $"{actor.FirstName} {actor.LastName} invalid");
                    var dbEntity = await _repository.GetActorByNameAsync(actor.FirstName, actor.LastName);
                    if (dbEntity == null) return Tuple.Create(false, $"{actor.FirstName} {actor.LastName} doesn't exist in db. Please add the actor first");
                    var entityToMap = film.Cast.Where(c => c.Actor.FirstName == actor.FirstName && c.Actor.LastName == actor.LastName).Select(c => c.Actor).SingleOrDefault();
                    entityToMap.Id = dbEntity.Id;
                }
            }
            return Tuple.Create(true, "");
        }
    }
}
