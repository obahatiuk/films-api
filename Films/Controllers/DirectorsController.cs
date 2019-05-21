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

        [HttpGet("{directorFirstName}/{directorLastName}")]
        public async Task<ActionResult<DirectorModel>> Get(string directorFirstName, string directorLastName)
        {
            try
            {
                if (string.IsNullOrEmpty(directorFirstName) || string.IsNullOrEmpty(directorLastName)) return BadRequest("Name is invalid");

                var director = await _repository.GetDirectorByNameAsync(directorFirstName, directorLastName);
                return _mapper.Map<DirectorModel>(director);
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
                var existingDirector = _repository.GetDirectorByNameAsync(model.FirstName, model.LastName);
                if (existingDirector != null) return BadRequest("There is director with the name in db");

                var director = _mapper.Map<Director>(model);
                _repository.Add(director);

                if (await _repository.SaveChangesAsync()) return Created($"api/Directors/{model.FirstName}/{model.LastName}", _mapper.Map<DirectorModel>(director));

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}