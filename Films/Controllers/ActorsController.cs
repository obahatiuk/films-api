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
                var existingActor = await _repository.GetActorByNameAsync(model.FirstName, model.LastName);
                if (existingActor != null) return BadRequest("The actor already exists");

                var actor = _mapper.Map<Actor>(model);
                var updateFilmsResult = await UpdateActorsFilms(actor);

                if (!updateFilmsResult.Item1) return BadRequest(updateFilmsResult.Item2);

                _repository.Add(actor);

                if (await _repository.SaveChangesAsync()) return Created($"api/Actors/{model.FirstName}/{model.LastName}", _mapper.Map<ActorModel>(actor));
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut("{firstName}_{lastName}")]
        public async Task<ActionResult<ActorModel>> Put(string firstName, string lastName, ActorModel model)
        {
            try
            {
                var actor = await _repository.GetActorByNameAsync(firstName, lastName, true);

                 _mapper.Map(model, actor);
                
                var updateFilmsResult = await UpdateActorsFilms(actor);

                if (!updateFilmsResult.Item1) return BadRequest(updateFilmsResult.Item2);
                

                if (await _repository.SaveChangesAsync()) return _mapper.Map<ActorModel>(actor);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception e)
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
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
             
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(string firstName, string lastName)
        {
            try
            {
                var actor = await _repository.GetActorByNameAsync(firstName, lastName);
                if (actor == null) return BadRequest($"Actor {firstName} {lastName} doesn't exist in db");

                _repository.Delete(actor);

                if (await _repository.SaveChangesAsync()) return Ok($"Actor {firstName} {lastName} was deleted");
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<Tuple<bool, string>>UpdateActorsFilms(Actor actor)
        {
            if (actor.ActorFilms.Count() != 0)
            {
                var films = actor.ActorFilms.Select(af => af.Film).ToArray();
                foreach (var film in films)
                {
                    if (string.IsNullOrEmpty(film.Title)) return Tuple.Create(false, "Film title cannot be null");
                    var existingEntity = await _repository.GetFilmByTitleAsync(film.Title);
                    if(existingEntity == null) return Tuple.Create(false, $"Film {film.Title} doesn't exist in db");
                    film.Id = existingEntity.Id;
                }
            }
            return Tuple.Create(true, "");
        }
    }
}