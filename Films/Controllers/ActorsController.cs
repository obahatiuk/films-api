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
                _repository.Add(actor);

                if (await _repository.SaveChangesAsync()) return Created($"api/Actors/{model.FirstName}/{model.LastName}", _mapper.Map<ActorModel>(actor));
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{actorFirstName}/{actorLastName}")]
        public async Task<ActionResult<ActorModel>> Get(string actorFirstName, string actorLastName)
        {
            try
            {
                if (string.IsNullOrEmpty(actorFirstName) || string.IsNullOrEmpty(actorLastName)) return BadRequest("Name is invalid");

                var actor = await _repository.GetActorByNameAsync(actorFirstName, actorLastName);
                return _mapper.Map<ActorModel>(actor);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
             
        }
    }
}