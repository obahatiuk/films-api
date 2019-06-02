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
                return _mapper.Map<DirectorModel[]>(directors);
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
                if (model.Films != null && model.Films.Count() > 0) return BadRequest("Please add director first. Than you can update list of films with PUT request");

                var filmsModelValidationResult = ModelsValidator.ValidateFilmModels(model);

                if (!filmsModelValidationResult.Item1) return BadRequest(filmsModelValidationResult.Item2);

                var existingDirector = await _repository.GetDirectorByNameAsync(model.FirstName, model.LastName);
                if (existingDirector != null) return BadRequest("There is director with the name in db");

                var director = _mapper.Map<Director>(model);

                _repository.Add(director);

                if (await _repository.SaveChangesAsync()) return Created($"api/Directors/{model.FirstName}/{model.LastName}", _mapper.Map<DirectorModel>(director));

                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");
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
                var filmsModelValidationResult = ModelsValidator.ValidateFilmModels(model);

                if (!filmsModelValidationResult.Item1) return BadRequest(filmsModelValidationResult.Item2);

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

                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpDelete("{firstName}_{lastName}")]
        public async Task<ActionResult> Delete(string firstName, string lastName)
        {
            try
            {
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)) return BadRequest("Name is not valid");

                var director = await _repository.GetDirectorByNameAsync(firstName, lastName);
                if (director == null) return BadRequest("Director doesn't exist in db");

                _repository.Delete(director);

                if (await _repository.SaveChangesAsync()) return Ok($"Director {firstName} {lastName} was deleted");
                return StatusCode(StatusCodes.Status500InternalServerError, "It looks like no changes were made");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        private async Task<Tuple<bool, string>> UpdateFilmForDirector(Director director)
        {
            if (director.Films != null && director.Films.Count() > 0)
            {
                var films = director.Films.ToArray();

                director.Films = null;

                for (int i = 0; i < films.Count(); i++)
                {
                    var film = films[i];

                    var existingEntity = await _repository.GetFilmByTitleAsync(film.Title);

                    if (existingEntity == null) return Tuple.Create(false, $"{film.Title} doesn't exist in db. Please add film first");

                    existingEntity.DirectorId = director.Id;
                    //director.Films.ElementAt(i).Id = existingEntity.Id;

                }
            }
            return Tuple.Create(true, "");
        }
    }
}