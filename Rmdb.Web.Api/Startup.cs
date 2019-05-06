
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rmdb.Domain.Services;
using Rmdb.Domain.Services.Impl;
using Rmdb.Domain.Services.Profiles;
using Rmdb.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rmdb.Web.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RmdbContext>(opt => opt.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            Mapper.Initialize(cfg => {
                cfg.AddProfile<MovieProfile>();
                cfg.AddProfile<ActorProfile>();
            });

            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;

                // status codes valid for each request
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));

                var jsonOutputFormatter = setupAction.OutputFormatters
                    .OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    // remove text/json as it isn't the approved media type
                    // for working with JSON at API level
                    if (jsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
                    {
                        jsonOutputFormatter.SupportedMediaTypes.Remove("text/json");
                    }
                }
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddTransient<IMovieService, MovieService>();
            services.AddTransient<IActorService, ActorService>();


            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc($"RmdbAPISpecification",
                        new Microsoft.OpenApi.Models.OpenApiInfo()
                        {
                            Title = "Realdolmen Movie Database API",
                            Version = "v1",
                            Description = "Through this API you can access the Realdolmen movie database"                         
                        }); 

                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

                setupAction.IncludeXmlComments(xmlCommentsFullPath);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint(
                    $"/swagger/RmdbAPISpecification/swagger.json",
                    "Realdolmen");

                setupAction.RoutePrefix = "";

                setupAction.DefaultModelRendering(ModelRendering.Model);
            });

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
