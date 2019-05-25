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

                var filmsUpdateResult = await UpdateDirectorsFilm(director);

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

                var filmsUpdateResult = await UpdateDirectorsFilm(director);

                if (!filmsUpdateResult.Item1) return BadRequest(filmsUpdateResult.Item2);

                if (await _repository.SaveChangesAsync()) return Created($"api/Directors/{model.FirstName}/{model.LastName}", _mapper.Map<DirectorModel>(director));

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        private async Task<Tuple<bool, string>> UpdateDirectorsFilm(Director director)
        {
            if (director.Films.Count() != 0)
            {
                foreach (var film in director.Films)
                {
                    if (string.IsNullOrEmpty(film.Title)) return Tuple.Create(false, $"Film title: {film.Title} is invalid");
                    var filmId = (await _repository.GetFilmByTitleAsync(film.Title)).Id;

                    if (filmId < 1) return Tuple.Create(false, $"Film {film.Title} doesn't exist in db. Please add film first");
                    film.Id = filmId;
                }
            }
            return Tuple.Create(true, "");
        }
    }
}