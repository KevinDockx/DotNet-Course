using System;

namespace Rmdb.Domain.Dtos.Actors
{
    public class ActorListDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? Deceased { get; set; }
    }
}
