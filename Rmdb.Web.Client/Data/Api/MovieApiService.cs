using AutoMapper;
using Marvin.StreamExtensions;
using Rmdb.Domain.Dtos.Actors;
using Rmdb.Domain.Dtos.Movies;
using Rmdb.Web.Client.Data.Contracts;
using Rmdb.Web.Client.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Rmdb.Web.Client.Data.Api
{
    public class MovieApiService : IMovieService
    { 
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;

        public MovieApiService(IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory)); ;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<MovieActor> AddActorAsync(Guid movieId, Guid actorId)
        {
            if (movieId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(movieId)} cannot be empty.", nameof(movieId));
            }
            if (actorId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(actorId)} cannot be empty.", nameof(actorId));
            }

            var actorToAdd = new AddActorToMovieDto()
            {
                ActorId = actorId
            };
 
            var client = _httpClientFactory.CreateClient("MoviesClient");

            var memoryContentStream = new MemoryStream();
            memoryContentStream.SerializeToJsonAndWrite(actorToAdd,
                          new UTF8Encoding(), 1024, true);
            memoryContentStream.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(
              HttpMethod.Put,
              $"api/movies/{movieId}/actors"))
            {
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                using (var streamContent = new StreamContent(memoryContentStream))
                {
                    request.Content = streamContent;
                    request.Content.Headers.ContentType =
                      new MediaTypeHeaderValue("application/json");

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        var stream = await response.Content.ReadAsStreamAsync();
                        return _mapper.Map<MovieActor>(stream.ReadAndDeserializeFromJson<ActorListDto>());
                    }
                }
            } 
        }

        public async Task<Movie> CreateAsync(Movie movie)
        {
            if (movie == null)
            {
                throw new ArgumentNullException(nameof(movie));
            }

            var client = _httpClientFactory.CreateClient("MoviesClient");

            var movieToAdd = _mapper.Map<AddMovieDto>(movie);

            var memoryContentStream = new MemoryStream();
            memoryContentStream.SerializeToJsonAndWrite(movieToAdd,
                          new UTF8Encoding(), 1024, true);
            memoryContentStream.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(
              HttpMethod.Post,
              $"api/movies"))
            {
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                using (var streamContent = new StreamContent(memoryContentStream))
                {
                    request.Content = streamContent;
                    request.Content.Headers.ContentType =
                      new MediaTypeHeaderValue("application/json");

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        var stream = await response.Content.ReadAsStreamAsync();
                        return _mapper.Map<Movie>(
                            stream.ReadAndDeserializeFromJson<MovieDetailDto>()); 
                    }
                }
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(id)} cannot be empty.", nameof(id));
            }

            var client = _httpClientFactory.CreateClient("MoviesClient");

            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/movies/{id}");
             
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            var client = _httpClientFactory.CreateClient("MoviesClient");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/movies");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); 

            using (var response = await client.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                return _mapper.Map<IEnumerable<Movie>>(
                    stream.ReadAndDeserializeFromJson<List<MovieDetailDto>>());
            }
        }

        public async Task<Movie> GetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(id)} cannot be empty.", nameof(id));
            }

            var client = _httpClientFactory.CreateClient("MoviesClient");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/movies/{id}");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await client.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                return _mapper.Map<Movie>(stream.ReadAndDeserializeFromJson<MovieDetailDto>());
            }
        }
         

        public Task<Movie> UpdateAsync(Guid id, Movie movie)
        {
            throw new NotImplementedException();
        }
    }
}
