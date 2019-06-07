
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rmdb.Domain.Services;
using Rmdb.Domain.Services.Impl;
using Rmdb.Domain.Services.Profiles;
using Rmdb.Infrastructure;

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

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddTransient<IMovieService, MovieService>();
            services.AddTransient<IActorService, ActorService>();

            // response compression (gzip)
            services.AddResponseCompression(options =>
            {
                // carefull: com­pres­sion in TLS is com­promised due to BREACH at­tack. 
                // How­ever, if your cook­ies fol­lows same-site pol­icy, your site is pro­tected against CSRF and BREACH at­tacks.
                // (source: https://dajbych.net/how-to-enable-response-compression-in-asp-net-core-2-with-gzip-and-brotli-encoding)
                // & https://brockallen.com/2019/01/11/same-site-cookies-asp-net-core-and-external-authentication-providers/
                options.EnableForHttps = true;
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

            app.UseMvc();
        }
    }
}
