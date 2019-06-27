using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rmdb.Web.Client.Data.Api;
using Rmdb.Web.Client.Data.Contracts;
using Rmdb.Web.Client.Data.SessionStorage;
using Rmdb.Web.Client.ViewModels.Actors;
using Rmdb.Web.Client.ViewModels.Movies;

namespace Rmdb.Web.Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));

            // added for demo purposes
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
            });

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            // services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpContextAccessor();
            services.AddTransient<IMovieService, MovieApiService>();
            services.AddTransient<IActorService, ActorApiService>();

            services.AddMvc(options =>
            {
                // add global authorization filter
                var policy = new AuthorizationPolicyBuilder()
                          .RequireAuthenticatedUser()
                          .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }
            ).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "RMDBCookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("RMDBCookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:44351";
                    options.RequireHttpsMetadata = true;

                    // Use the hybrid grant, but ensure access tokens aren't exposed
                    // via the front channel
                    options.ResponseType = "code id_token";
                    options.ClientId = "rmdbwebclient";
                    // client secret required for token endpoint access
                    options.ClientSecret = "2E51842C-56EF-481A-938C-A0C4BF648215";
                    // always get claims from the userinfo endpoint (to avoid URL length restrictions)
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    #region add scope to access API
                    // options.Scope.Add("rmdbapi");
                    #endregion

                    #region add scope to get a refresh token
                    // options.Scope.Add("offline_access");
                    #endregion

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenResponseReceived = async ctx =>
                        {
                            Debug.WriteLine(ctx.TokenEndpointResponse.IdToken);
                            Debug.WriteLine(ctx.TokenEndpointResponse.AccessToken);
                            Debug.WriteLine(ctx.TokenEndpointResponse.RefreshToken);
                            await Task.Yield();
                        },
                        OnTicketReceived = async ctx =>
                        {
                            // Invoked after the remote ticket has been received.
                            // Can be used to modify the Principal before it is passed to the Cookie scheme for sign-in.
                            // => claims tranformation

                            // Sample of manipulating the claimsidentity
                            //if (ctx.Principal.Identity is ClaimsIdentity identity)
                            //{
                            //    ctx.Principal.FindAll(x => x.Type == "groups")
                            //        .ToList()
                            //        .ForEach(identity.RemoveClaim);
                            //}
                            await Task.Yield();
                        }

                    };
                });

            
            services.AddHttpClient("MoviesClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:52330/");
                client.Timeout = new TimeSpan(0, 0, 30);
                client.DefaultRequestHeaders.Clear();
                // all requests = gzip, so it's safe to add it as a default header
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            })
              .ConfigurePrimaryHttpMessageHandler(handler =>
                    new HttpClientHandler()
                    {
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip
                    });

            services.AddHttpClient("ActorsClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:52330/");
                client.Timeout = new TimeSpan(0, 0, 30);
                client.DefaultRequestHeaders.Clear();
            });


            #region TokenAuthenticationHandler
            // Build an intermediate service provider
            //var intermediateServiceProvider = services.BuildServiceProvider();

            //services.AddHttpClient("MoviesClient", client =>
            //{
            //    client.BaseAddress = new Uri("http://localhost:52330/");
            //    client.Timeout = new TimeSpan(0, 0, 30);
            //    client.DefaultRequestHeaders.Clear();
            //    // all requests = gzip, so it's safe to add it as a default header
            //    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            //}).AddHttpMessageHandler(handler =>
            //    new TokenAuthenticationHandler(intermediateServiceProvider.GetService<IHttpContextAccessor>()))
            //  .ConfigurePrimaryHttpMessageHandler(handler =>
            //        new HttpClientHandler()
            //        {
            //            AutomaticDecompression = System.Net.DecompressionMethods.GZip
            //        });

            //services.AddHttpClient("ActorsClient", client =>
            //{
            //    client.BaseAddress = new Uri("http://localhost:52330/");
            //    client.Timeout = new TimeSpan(0, 0, 30);
            //    client.DefaultRequestHeaders.Clear();
            //}).AddHttpMessageHandler(handler =>
            //    new TokenAuthenticationHandler(intermediateServiceProvider.GetService<IHttpContextAccessor>()));
            #endregion

            #region Token Authentication with Token Refresh Handler
           // services.AddHttpClient("IdentityServerClient", client =>
           // {
           //     client.BaseAddress = new Uri("https://localhost:44351");
           //     client.Timeout = new TimeSpan(0, 0, 30);
           //     client.DefaultRequestHeaders.Clear();
           //     client.DefaultRequestHeaders.Add("Accept", "application/json");
           // });

           // // Build an intermediate service provider
           // var intermediateServiceProvider = services.BuildServiceProvider();

           // services.AddHttpClient("MoviesClient", client =>
           // {
           //     client.BaseAddress = new Uri("http://localhost:52330/");
           //     client.Timeout = new TimeSpan(0, 0, 30);
           //     client.DefaultRequestHeaders.Clear();
           //     // all requests = gzip, so it's safe to add it as a default header
           //     client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
           // }).AddHttpMessageHandler(handler =>
           //     new TokenAuthenticationWithTokenRefreshHandler( 
           //         intermediateServiceProvider.GetService<IHttpClientFactory>(),
           //         intermediateServiceProvider.GetService<IHttpContextAccessor>()))
           //.ConfigurePrimaryHttpMessageHandler(handler =>
           //      new HttpClientHandler()
           //      {
           //          AutomaticDecompression = System.Net.DecompressionMethods.GZip
           //      });

           // services.AddHttpClient("ActorsClient", client =>
           // {
           //     client.BaseAddress = new Uri("http://localhost:52330/");
           //     client.Timeout = new TimeSpan(0, 0, 30);
           //     client.DefaultRequestHeaders.Clear();
           // }).AddHttpMessageHandler(handler =>
           //     new TokenAuthenticationWithTokenRefreshHandler( 
           //         intermediateServiceProvider.GetService<IHttpClientFactory>(),
           //         intermediateServiceProvider.GetService<IHttpContextAccessor>()));
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            // for demo purposes
            app.UseSession();
             
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
