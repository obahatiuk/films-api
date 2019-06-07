using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Films.Core;
using Films.Data;
using Films.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Films.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActorsController : ControllerBase
    {
        private readonly IFilmRepository _repository;
        private readonly IMapper _mapper;

        public ActorsController(IFilmRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ActorModel>> Post(ActorModel model)
        {
            try
            {
                if(model.Films != null && model.Films.Count() > 0) return BadRequest("Add actor first. If you want to add films update actor");

                var existingActor = await _repository.GetActorByNameAsync(model.FirstName, model.LastName);
                if (existingActor != null) return BadRequest("The actor already exists");

                var actor = _mapper.Map<Actor>(model);

                _repository.Add(actor);

                if (await _repository.SaveChangesAsync())
                {
                    actor = await _repository.GetActorByNameAsync(actor.FirstName, actor.LastName); 
                    return Created($"api/Actors/{model.FirstName}/{model.LastName}", _mapper.Map<ActorModel>(actor));
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut("{firstName}_{lastName}")]
        public async Task<ActionResult<ActorModel>> Put(string firstName, string lastName, ActorModel model)
        {
            try
            {
                var filmsModelValidationResult = ModelsValidator.ValidateFilmModels(model);

                if (!filmsModelValidationResult.Item1) return BadRequest(filmsModelValidationResult.Item2);

                var actor = await _repository.GetActorByNameAsync(firstName, lastName);

                if (actor == null) return BadRequest($"Actor {firstName} {lastName} doesn't exist");

                 _mapper.Map(model, actor);
                
                var updateFilmsResult = await UpdateActorsFilms(actor);

                if (!updateFilmsResult.Item1)
                {
                    return BadRequest(updateFilmsResult.Item2);
                }


                if (await _repository.SaveChangesAsync())
                {
                    actor = await _repository.GetActorByNameAsync(firstName, lastName, true); 
                    return _mapper.Map<ActorModel>(actor);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet]
        public async Task<ActionResult<ActorModel[]>> Get(bool includeFilms = false)
        {
            try
            {
                var actors = await _repository.GetAllActorsAsync(includeFilms);
                return _mapper.Map<ActorModel[]>(actors);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
             
        }

        [HttpDelete("{firstName}_{lastName}")]
        public async Task<ActionResult> Delete(string firstName, string lastName)
        {
            try
            {
                var actor = await _repository.GetActorByNameAsync(firstName, lastName);
                if (actor == null) return BadRequest($"Actor {firstName} {lastName} doesn't exist in db");

                _repository.Delete(actor);

                if (await _repository.SaveChangesAsync()) return Ok($"Actor {firstName} {lastName} was deleted");
                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        private async Task<Tuple<bool, string>>UpdateActorsFilms(Actor actor)
        {
            if (actor.ActorFilms != null && actor.ActorFilms.Count() > 0)
            {
                var films = actor.ActorFilms.Select(af => af.Film).ToArray();

                actor.ActorFilms = new List<ActorFilm>();

                foreach (var film in films)
                {
                    if (string.IsNullOrEmpty(film.Title)) return Tuple.Create(false, "Film title cannot be null");

                    var existingFilmEntity = await _repository.GetFilmByTitleAsync(film.Title, includeActorFilm: true);

                    if(existingFilmEntity == null) return Tuple.Create(false, $"Film {film.Title} doesn't exist in db");

                    if (!existingFilmEntity.Cast.Where(c => c.ActorId == actor.Id).Any() || actor.Id <= 0) existingFilmEntity.Cast.Add(new ActorFilm() { ActorId = actor.Id, FilmId = existingFilmEntity.Id });
                        
                    film.Id = existingFilmEntity.Id;
                }
            }
            return Tuple.Create(true, "");
        }

    }
}