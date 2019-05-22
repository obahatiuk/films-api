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
        public async Task<ActionResult<FilmModel[]>> Get()
        {
            try
            {
                var results = await _repository.GetAllFilmsAsync(true, true);

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
                var existingFilm = await _repository.GetFilmByTitleAsync(model.Title);
                if (existingFilm != null) return BadRequest("There is a film with same name");
                if (model.Director.FirstName == null || model.Director.LastName == null) return BadRequest("There is no director provided");

                var director = await _repository.GetDirectorByNameAsync(model.Director.FirstName, model.Director.LastName);
                if (director == null) return BadRequest("Director not found");

                if (model.Cast != null)
                {
                    foreach (var actorModel in model.Cast)
                    {
                        if (string.IsNullOrEmpty(actorModel.FirstName) || string.IsNullOrEmpty(actorModel.LastName)) return BadRequest($"Actor's name: {actorModel.FirstName} && {actorModel.LastName} is invalid");
                        if (await _repository.GetActorByNameAsync(actorModel.FirstName, actorModel.LastName) == null) return BadRequest($"Actor {actorModel.FirstName} {actorModel.LastName} does not exist in db");
                    }
                }

                var film = _mapper.Map<Film>(model);
                film.DirectorId = director.Id;
                _repository.Add(film);

                if (await _repository.SaveChangesAsync()) return Created($"/api/films/{model.Title}", _mapper.Map<FilmModel>(film));

                foreach (var actorModel in model.Cast)
                {
                    var actor = await _repository.GetActorByNameAsync(actorModel.FirstName, actorModel.LastName);
                    var existingActors = await _repository.GetActorsByFilmAsync(film.Id);

                    if (existingActors.Where(a => a.Id == actor.Id) == null) _repository.Add(new ActorFilm() { ActorId = actor.Id, FilmId = film.Id });
                }

                if (await _repository.SaveChangesAsync()) return Created($"/api/films/{model.Title}", _mapper.Map<FilmModel>(film));

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut("{filmName}")]
        public async Task<ActionResult<FilmModel>> UpdateFilm(string filmName, FilmModel model)
        {
            try
            {
                var film = await _repository.GetFilmByTitleAsync(filmName);
                if (film == null) return BadRequest("Film doesn't exist in db please add film with post method /api/films");

                _mapper.Map(model, film);

                if (await _repository.SaveChangesAsync()) return _mapper.Map<FilmModel>(film);

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut("{filmName}")]
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
    }
}
