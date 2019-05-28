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
    public class DirectorsController : ControllerBase
    {
        private readonly IFilmRepository _repository;
        private readonly IMapper _mapper;

        public DirectorsController(IFilmRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<DirectorModel[]>> Get(bool includeFilms = false)
        {
            try
            {
                var directors = await _repository.GetAllDirectorsAsync(includeFilms);
                var x = _mapper.Map<DirectorModel[]>(directors);
                return x;
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<DirectorModel>> Post(DirectorModel model)
        {
            try
            {
                var existingDirector = await _repository.GetDirectorByNameAsync(model.FirstName, model.LastName);
                if (existingDirector != null) return BadRequest("There is director with the name in db");

                if (model.Films != null && model.Films.Count() > 0) return BadRequest("Please add director first. Than you can update list of films with PUT request");

                var director = _mapper.Map<Director>(model);

                _repository.Add(director);

                //var filmsUpdateResult = await UpdateFilmForDirector(director);

                //if (!filmsUpdateResult.Item1)
                //{
                //    //_repository.UndoChanges();
                //    return BadRequest(filmsUpdateResult.Item2);
                //}

                if (!await _repository.SaveChangesAsync())
                {
                    //_repository.UndoChanges();
                    return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
                } 

                if (await _repository.SaveChangesAsync()) return Created($"api/Directors/{model.FirstName}/{model.LastName}", _mapper.Map<DirectorModel>(director));

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut("{firstName}_{lastName}")]
        public async Task<ActionResult<DirectorModel>> Put(string firstName, string lastName, DirectorModel model)
        {
            try
            {
                var director = await _repository.GetDirectorByNameAsync(firstName, lastName);
                if (director == null) return BadRequest("There is director with the name in db");

                _mapper.Map(model, director);

                var filmsUpdateResult = await UpdateFilmForDirector(director);

                if (!filmsUpdateResult.Item1)
                {
                    //_repository.Undo(director);
                    return BadRequest(filmsUpdateResult.Item2);
                }

                if (await _repository.SaveChangesAsync()) return Created($"api/Directors/{model.FirstName}/{model.LastName}", _mapper.Map<DirectorModel>(director));

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpDelete("{firstName_lastName}")]
        public async Task<ActionResult> Delete(string firstName, string lastName)
        {
            try
            {
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)) return BadRequest("Name is not valid");

                var director = _repository.GetDirectorByNameAsync(firstName, lastName);
                if (director == null) return BadRequest("Director doesn't exist in db");

                _repository.Delete(director);

                if (await _repository.SaveChangesAsync()) return Ok($"Director {firstName} {lastName} was deleted");
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        private async Task<Tuple<bool, string>> UpdateFilmForDirector(Director director)
        {
            for(int i =0; i < director.Films.Count(); i++)
            {
                var film = director.Films.ElementAt(i);
                if (film.Cast != null && film.Cast.Count() > 0) return Tuple.Create(false, $"{film.Title} contains Cast inforamtion. Please use film controller in order to update cast");
                var existingEntity = await _repository.GetFilmByTitleAsync(film.Title);
                if (existingEntity == null) return Tuple.Create(false, $"{film.Title} doesn't exist in db. Please add film first");
                existingEntity.DirectorId = director.Id;
                director.Films.Remove(film);
            }
            return Tuple.Create(true, "");
        }
    }
}