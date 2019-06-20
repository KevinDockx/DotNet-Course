using AutoMapper;
using Marvin.StreamExtensions;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
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
        private HttpClient _httpClient = new HttpClient();

        // enable automatic decompression
        //private HttpClient _httpClient = new HttpClient(
        // new HttpClientHandler()
        // {
        //     AutomaticDecompression = System.Net.DecompressionMethods.GZip
        // });


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;

        public MovieApiService(IMapper mapper)
        {
            _mapper = mapper ?? 
                throw new ArgumentNullException(nameof(mapper));

            // set up httpclient defaults
            _httpClient.BaseAddress = new Uri("http://localhost:52330/");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        #region Constructor with HttpClientFactory
        //public MovieApiService(IHttpClientFactory httpClientFactory, IMapper mapper)
        //{
        //    _httpClientFactory = httpClientFactory ??
        //        throw new ArgumentNullException(nameof(httpClientFactory));
        //    _mapper = mapper ??
        //        throw new ArgumentNullException(nameof(mapper)); 
        //}
        #endregion

        #region GetAllAsync 
        //public async Task<IEnumerable<Movie>> GetAllAsync()
        //{             
        //    var request = new HttpRequestMessage(
        //        HttpMethod.Get,
        //       "api/movies");

        //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    var response = await _httpClient.SendAsync(request);
        //    response.EnsureSuccessStatusCode();

        //    var content = await response.Content.ReadAsStringAsync();
        //    var movies = JsonConvert.DeserializeObject<IEnumerable<MovieListDto>>(content);
        //    return _mapper.Map<IEnumerable<Movie>>(movies); 
        //}
        #endregion

        #region GetAllAsync with streams
        //public async Task<IEnumerable<Movie>> GetAllAsync()
        //{
        //    var request = new HttpRequestMessage(
        //        HttpMethod.Get,
        //       "api/movies");

        //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    using (var response = await _httpClient.SendAsync(request))
        //    {
        //        response.EnsureSuccessStatusCode();

        //        var stream = await response.Content.ReadAsStreamAsync();

        //        using (var streamReader = new StreamReader(stream))
        //        {
        //            using (var jsonTextReader = new JsonTextReader(streamReader))
        //            {
        //                var jsonSerializer = new JsonSerializer();
        //                var movies = jsonSerializer.Deserialize<IEnumerable<MovieListDto>>(jsonTextReader);
        //                return _mapper.Map<IEnumerable<Movie>>(movies);
        //            }
        //        }
        //    }              
        //}
        #endregion

        #region GetAllAsync with streams (response headers read)
        //public async Task<IEnumerable<Movie>> GetAllAsync()
        //{
        //    var request = new HttpRequestMessage(
        //        HttpMethod.Get,
        //       "api/movies");

        //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    using (var response = await _httpClient.SendAsync(request, 
        //        HttpCompletionOption.ResponseHeadersRead))
        //    {
        //        response.EnsureSuccessStatusCode();

        //        var stream = await response.Content.ReadAsStreamAsync();

        //        using (var streamReader = new StreamReader(stream))
        //        {
        //            using (var jsonTextReader = new JsonTextReader(streamReader))
        //            {
        //                var jsonSerializer = new JsonSerializer();
        //                var movies = jsonSerializer.Deserialize<IEnumerable<MovieListDto>>(jsonTextReader);
        //                return _mapper.Map<IEnumerable<Movie>>(movies);
        //            }
        //        }
        //    }
        //}
        #endregion

        #region GetAllAsync with streams (response headers read)
        //public async Task<IEnumerable<Movie>> GetAllAsync()
        //{
        //    var request = new HttpRequestMessage(
        //        HttpMethod.Get,
        //       "api/movies");

        //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    using (var response = await _httpClient.SendAsync(request,
        //        HttpCompletionOption.ResponseHeadersRead))
        //    {
        //        var stream = await response.Content.ReadAsStreamAsync();
        //        response.EnsureSuccessStatusCode();

        //        var movies = stream.ReadAndDeserializeFromJson<List<MovieListDto>>();
        //        return _mapper.Map<IEnumerable<Movie>>(movies);
        //    }
        //}
        #endregion

        #region GetAllAsync with streams (response headers read) + compression
        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
               "api/movies");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();

                var movies = stream.ReadAndDeserializeFromJson<List<MovieListDto>>();
                return _mapper.Map<IEnumerable<Movie>>(movies);
            }
        }
        #endregion



        #region GetAllAsync with streams & client factory (ext methods)
        //public async Task<IEnumerable<Movie>> GetAllAsync()
        //{
        //    var client = _httpClientFactory.CreateClient("MoviesClient");

        //    var request = new HttpRequestMessage(
        //        HttpMethod.Get,
        //        "api/movies");

        //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    using (var response = await client.SendAsync(request,
        //        HttpCompletionOption.ResponseHeadersRead))
        //    {
        //        var stream = await response.Content.ReadAsStreamAsync();
        //        response.EnsureSuccessStatusCode();

        //        var movies = stream.ReadAndDeserializeFromJson<List<MovieListDto>>();
        //        return _mapper.Map<IEnumerable<Movie>>(movies);
        //    }
        //}
        #endregion

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
                return _mapper.Map<Movie>(stream.ReadAndDeserializeFromJson<MovieDetailWithActorsDto>());
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
         

        public async Task<Movie> UpdateAsync(Guid id, Movie movie)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            if (movie == null)
            {
                throw new ArgumentNullException(nameof(movie));
            }
            
            // note that in a real-life situation, a diff between the originally loaded
            // object & the one you made your changes on is the way to go (or use self-tracking
            // object).  In the demo we haven't got that kind of functionality

            // create a patch document
            var patchDoc = new JsonPatchDocument<EditMovieDto>();
            patchDoc.Replace(m => m.Color, movie.Color);
            patchDoc.Replace(m => m.Description, movie.Description);
            patchDoc.Replace(m => m.ReleaseDate, movie.ReleaseDate);
            patchDoc.Replace(m => m.RunTime, movie.RunTime);
            patchDoc.Replace(m => m.Score, movie.Score);
            patchDoc.Replace(m => m.Title, movie.Title);
 
            var client = _httpClientFactory.CreateClient("MoviesClient");

            var memoryContentStream = new MemoryStream();
            memoryContentStream.SerializeToJsonAndWrite(patchDoc,
                          new UTF8Encoding(), 1024, true);
            memoryContentStream.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(
              HttpMethod.Patch,
              $"api/movies/{id}"))
            {
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                using (var streamContent = new StreamContent(memoryContentStream))
                {
                    request.Content = streamContent;
                    request.Content.Headers.ContentType =
                      new MediaTypeHeaderValue("application/json-patch+json");

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
              HttpMethod.Post,
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
    }
}
