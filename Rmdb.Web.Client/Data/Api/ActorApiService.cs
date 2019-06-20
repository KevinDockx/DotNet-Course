using AutoMapper;
using Marvin.StreamExtensions;
using Rmdb.Domain.Dtos.Actors;
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
    public class ActorApiService : IActorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;

        public ActorApiService(IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory)); ;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task AddAsync(Actor person)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }

            var client = _httpClientFactory.CreateClient("ActorsClient");

            var actorToAdd = _mapper.Map<AddActorDto>(person);

            var memoryContentStream = new MemoryStream();
            memoryContentStream.SerializeToJsonAndWrite(actorToAdd,
                          new UTF8Encoding(), 1024, true);
            memoryContentStream.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(
              HttpMethod.Post,
              $"api/actors"))
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

                        // service doesn't need response content, otherwise you could
                        // use code like this:
                        //var stream = await response.Content.ReadAsStreamAsync();
                        //var actorDetail = stream.ReadAndDeserializeFromJson<ActorDetailDto>(); 
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

            var client = _httpClientFactory.CreateClient("ActorsClient");

            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/actors/{id}");

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<Actor>> GetAllAsync()
        {
            var client = _httpClientFactory.CreateClient("ActorsClient");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/actors");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await client.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();

                var actorList = stream.ReadAndDeserializeFromJson<List<ActorListDto>>();
                var mappedActorList = _mapper.Map<IEnumerable<Actor>>(actorList);
                return mappedActorList;
            }
        }

        public async Task<Actor> GetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(id)} cannot be empty.", nameof(id));
            }

            var client = _httpClientFactory.CreateClient("ActorsClient");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/actors/{id}");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await client.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                return _mapper.Map<Actor>(stream.ReadAndDeserializeFromJson<ActorDetailDto>()); 
            }
        }

        public void Save()
        {
            // no code
        }

        public Task<Actor> UpdateAsync(Guid id, Actor actor)
        {
            throw new NotImplementedException();
        }
    }
}
