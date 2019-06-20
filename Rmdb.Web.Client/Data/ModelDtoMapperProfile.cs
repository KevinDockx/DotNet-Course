using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rmdb.Web.Client.Data
{
    public class ModelDtoMapperProfile : Profile
    {
        public ModelDtoMapperProfile()
        {
            CreateMap<Domain.Dtos.Actors.ActorListDto, Model.MovieActor>()
                .ForMember(d => d.ActorId, o => o.MapFrom(s => s.Id));
                
            CreateMap<Model.Movie, Domain.Dtos.Movies.AddMovieDto>();
            CreateMap<Domain.Dtos.Movies.MovieDetailDto, Model.Movie>();
            CreateMap<Domain.Dtos.Movies.MovieDetailWithActorsDto, Model.Movie>();
            CreateMap<Domain.Dtos.Movies.MovieListDto, Model.Movie>();

            CreateMap<Model.Actor, Domain.Dtos.Actors.AddActorDto>();
            CreateMap<Domain.Dtos.Actors.ActorListDto, Model.Actor>();
            CreateMap<Domain.Dtos.Actors.ActorDetailDto, Model.Actor>();
        }
    }
}
