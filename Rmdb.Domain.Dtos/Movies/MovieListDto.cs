using System;

namespace Rmdb.Domain.Dtos.Movies
{
 
    public class MovieListDto
    {
        public Guid Id { get; set; }         
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; } 
        public double Score { get; set; }
    }
}
