using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Rmdb.Domain.Dtos.Movies;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rmdb.Web.Api.OperationFilters
{
    public class GetMovieOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {         
            if (operation.OperationId != "GetMovieWithActors")
            {
                return;
            }

            operation.Responses[StatusCodes.Status200OK.ToString()].Content.Add(
                "application/vnd.rmdb.movie+json",
                new OpenApiMediaType()
                {
                    Schema = context.SchemaRegistry.GetOrRegister(typeof(MovieDetailDto))
                });
        }
    }
}
