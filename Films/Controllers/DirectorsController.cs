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

                var director = _mapper.Map<Director>(model);

                var filmsUpdateResult = UpdateDirectorsFilm(director);

                if (!filmsUpdateResult.Item1) return BadRequest(filmsUpdateResult.Item2);

                _repository.Add(director);

                if (await _repository.SaveChangesAsync()) return Created($"api/Directors/{model.FirstName}/{model.LastName}", _mapper.Map<DirectorModel>(director));

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut("{filmTitle}")]
        public async Task<ActionResult<DirectorModel>> Post(string filmTitle, DirectorModel model)
        {
            try
            {
                var director = await _repository.GetDirectorByNameAsync(model.FirstName, model.LastName);
                if (director != null) return BadRequest("There is director with the name in db");

                _mapper.Map(model, director);

                var filmsUpdateResult = UpdateDirectorsFilm(director);

                if (!filmsUpdateResult.Item1) return BadRequest(filmsUpdateResult.Item2);

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

        private Tuple<bool, string> UpdateDirectorsFilm(Director director)
        {
            if (director.Films != null && director.Films.Count() != 0) return Tuple.Create(false, $"This method doesn't support adding films to director instance. Please set the film's director by updating the film.");
            return Tuple.Create(true, "");
        }
    }
}