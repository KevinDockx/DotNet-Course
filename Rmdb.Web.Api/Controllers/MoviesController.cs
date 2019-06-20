using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Rmdb.Domain.Dtos.Movies;
using Rmdb.Domain.Services;
using Rmdb.Web.Api.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rmdb.Web.Api.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IMapper _mapper;

        public MoviesController(IMovieService movieService, 
            IMapper mapper)
        {
            _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

 
        /// <summary>
        /// Get a list of movies
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MovieListDto>>> Get()
        {
            return Ok(await _movieService.GetAsync());
        }

        // GET api/movies/{id}
        //[HttpGet("{id:Guid}")]
        //[RequestHeaderMatchesMediaType(HeaderNames.Accept,
        //    "application/vnd.rmdb.movie+json")]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //// [ApiExplorerSettings(IgnoreApi = true)]
        //public async Task<ActionResult<MovieDetailDto>> GetMovie(Guid id)
        //{
        //    if (id == Guid.Empty)
        //    {
        //        return BadRequest();
        //    }

        //    var movie = await _movieService.GetAsync(id);

        //    if (movie == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(movie);
        //}
      
        [HttpGet("{id:Guid}", Name = "GetMovieWithActors")]
        [RequestHeaderMatchesMediaType(HeaderNames.Accept,
            "*/*",
            "application/json",
            "application/vnd.rmdb.moviewithactors+json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MovieDetailWithActorsDto>> GetMovieWithActors(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var movie = await _movieService.GetWithActorsAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        // POST api/movies
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddMovieDto addMovie)
        {
            var id = await _movieService.AddAsync(addMovie);

            // TODO: get movie to return as response body
            var movieToReturn = await _movieService.GetAsync(id);

            return CreatedAtRoute("GetMovieWithActors", 
                new { id }, _mapper.Map<MovieDetailDto>(movieToReturn));
        }

        // PUT api/movies/{id}
        [HttpPut("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(Guid id, [FromBody] EditMovieDto editMovie)
        {
            var movie = await _movieService.UpdateAsync(id, editMovie);

            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        [HttpPatch("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary), 
            StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Patch(Guid id, 
            [FromBody] JsonPatchDocument<EditMovieDto> jsonPatchDocument)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            // patch is applied on the DTO
            // - get movie                    

            var movie = await _movieService.GetAsync(id); 

            if (movie == null)
            {
                return NotFound();
            }

            // - map to DTO
            var editMovieDto = _mapper.Map<EditMovieDto>(movie);

            // - apply patch
            jsonPatchDocument.ApplyTo(editMovieDto, ModelState);

            // - validate the DTO
            TryValidateModel(editMovieDto);

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            // - update changes   
            return Ok(await _movieService.UpdateAsync(id, editMovieDto));           
        }

        // DELETE api/movies/{id}
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _movieService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST api/movies/{id}/actors
        [HttpPost("{id:Guid}/actors")]
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
