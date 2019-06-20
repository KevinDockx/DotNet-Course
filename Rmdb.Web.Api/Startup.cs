
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
using Rmdb.Web.Api.OperationFilters;
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
            services.AddDbContext<RmdbContext>(opt =>
            opt.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc(setupAction =>
                 {
                     #region Global response type filters
                     setupAction.Filters.Add(
                         new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
                     setupAction.Filters.Add(
                         new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
                     setupAction.Filters.Add(
                         new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
                     setupAction.Filters.Add(
                         new ProducesDefaultResponseTypeAttribute());
                     #endregion

                     setupAction.ReturnHttpNotAcceptable = true;

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

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IActorService, ActorService>();

            // response compression (gzip)
            services.AddResponseCompression(options =>
            {
                // carefull: com­pres­sion in TLS is com­promised due to BREACH at­tack. 
                // How­ever, if your cook­ies fol­lows same-site pol­icy, your site is pro­tected against CSRF and BREACH at­tacks.
                // (source: https://dajbych.net/how-to-enable-response-compression-in-asp-net-core-2-with-gzip-and-brotli-encoding)
                // & https://brockallen.com/2019/01/11/same-site-cookies-asp-net-core-and-external-authentication-providers/
                options.EnableForHttps = true;
            });

            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc(
                    "RMDBOpenAPISpecification",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "RMDB API",
                        Version = "1",
                        Description = "Through this API you can access movies and actors.",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email = "mscommunity@realdolmen.com",
                            Name = "MS Community" 
                        },
                        License = new Microsoft.OpenApi.Models.OpenApiLicense()
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensource.org/licenses/MIT")
                        }
                    });
                
                #region OperationFilters
                setupAction.OperationFilter<GetMovieOperationFilter>();
                #endregion

                #region Multiple xml comment files
                var baseDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                foreach (var fileInfo in baseDirectoryInfo.EnumerateFiles("*.xml"))
                {
                    setupAction.IncludeXmlComments(fileInfo.FullName);
                }
                #endregion
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint(
                    "/swagger/RMDBOpenAPISpecification/swagger.json",
                    "RMDB API");
                setupAction.RoutePrefix = "";
            });

            app.UseHttpsRedirection();

            app.UseMvc();
        }
    }
}
