﻿
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rmdb.Domain.Dtos.Actors;
using Rmdb.Domain.Model;
using Rmdb.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rmdb.Domain.Services.Impl
{
    public class ActorService : IActorService
    {
        private readonly RmdbContext _ctx;

        public ActorService(RmdbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<IEnumerable<ActorListDto>> GetAsync()
        {
            return await _ctx.Actors.ProjectTo<ActorListDto>().ToListAsync();
        }

        public async Task<ActorDetailDto> GetAsync(Guid id)
        {
            return await _ctx.Actors
                .ProjectTo<ActorDetailDto>()
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Guid> AddAsync(AddActorDto addActor)
        {
            var newActor = new Person(addActor.Name, addActor.LastName);

            await _ctx.Actors.AddAsync(newActor);

            await _ctx.SaveChangesAsync();

            return newActor.Id;
        }

        public async Task<ActorDetailDto> UpdateAsync(Guid id, EditActorDto editActor)
        {
            var actor = await _ctx.Actors.FindAsync(id);

            if (actor == null)
            {
                return null;
            }

            actor.Name = editActor.Name;
            actor.LastName = editActor.LastName;
            actor.BirthDate = editActor.BirthDate;
            actor.Deceased = editActor.Deceased;

            await _ctx.SaveChangesAsync();

            return Mapper.Map<ActorDetailDto>(actor);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var actor = await _ctx.Actors.FindAsync(id);

            if (actor == null)
            {
                return false;
            }

            _ctx.Actors.Remove(actor);

            await _ctx.SaveChangesAsync();

            return true;
        }
    }
}
