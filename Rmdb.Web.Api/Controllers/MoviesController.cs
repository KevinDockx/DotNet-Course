﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rmdb.Domain.Dtos.Movies;
using Rmdb.Domain.Services;
using System;
using System.Threading.Tasks;

namespace Rmdb.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // GET api/movies
        [HttpGet] 
        [Produces("application/json", "application/vnd.rmdb.movie.v1+json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            return Ok(await _movieService.GetAsync());
        }

        // GET api/movies/{id}
        [HttpGet("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json", "application/vnd.rmdb.movie.v1+json")]
        public async Task<IActionResult> Get(Guid id)
        {
            var movie = await _movieService.GetAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        // POST api/movies
        [HttpPost]
        [Consumes(
            "application/json",
            "application/vnd.rmdb.movietoadd.v1+json")] 
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity,
            Type = typeof(ValidationProblemDetails))]
        [Produces(
            "application/json",
            "application/vnd.rmdb.movie.v1+json")] 
        public async Task<ActionResult> Post([FromBody] AddMovieDto addMovie)
        {
            var id = await _movieService.AddAsync(addMovie);

            return CreatedAtAction("Get", new { Id = id });
        }

        // PUT api/movies/{id}
        [HttpPut("{id:Guid}")]
        [Consumes(
            "application/json",
            "application/vnd.rmdb.movietoedit.v1+json")] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity,
            Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces(
            "application/json",
            "application/vnd.rmdb.movie.v1+json")]
        public async Task<IActionResult> Put(Guid id, [FromBody] EditMovieDto editMovie)
        {
            var movie = await _movieService.UpdateAsync(id, editMovie);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        // DELETE api/movies/{id}
        [HttpDelete("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _movieService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT api/movies/{id}/actors
        [HttpPut("{id:Guid}/actors")]
        public async Task<IActionResult> AddActor(Guid id, [FromBody]AddActorToMovieDto addActor)
        {
            var actor = await _movieService.AddActorToMovieAsync(id, addActor);

            if(actor == null)
            {
                return NotFound();
            }

            return Ok(actor);
        }
    }
}
