using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using Newtonsoft.Json;

namespace MyCodeCamp
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            _env = env;
            _config = config;
        }

        private IConfiguration _config { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_config);
            services.AddDbContext<CampContext>(options =>
            {
                options.UseSqlServer(_config.GetConnectionString("DefaultConnection"));
            });
//            services.AddCors(cfg =>
//            {
//                cfg.AddPolicy("Wildermuth", bldr =>
//                {
//                    bldr.AllowAnyHeader()
//                        .AllowAnyMethod()
//                        .WithOrigins("htttp://wildermuth.com");
//                });
//
//                cfg.AddPolicy("AnyGET", bldr =>
//                {
//                    bldr.AllowAnyHeader()
//                        .WithMethods("GET")
//                        .AllowAnyOrigin();
//                });
//            });
            services.AddMvc(opt =>
                    {
                        if (!_env.IsProduction()) opt.SslPort = 44380;
                        opt.Filters.Add(new RequireHttpsAttribute());
                    }
                )
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddScoped<ICampRepository, CampRepository>();
            services.AddTransient<CampDbInitializer>();
            services.AddTransient<CampIdentityInitializer>();

            services.AddAutoMapper();

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Tokens:Key"]));

            services.AddAuthentication( sharedOptions =>
                {
                    sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    sharedOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    sharedOptions.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opts =>
                {
                    opts.RequireHttpsMetadata = false;
                    opts.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ValidateIssuer = true,
                        ValidIssuer = _config["Tokens:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = _config["Tokens:Audience"],
                        ValidateLifetime = true
                        
                    };

                });


            services.AddIdentity<CampUser, IdentityRole>(opt => { })
                .AddEntityFrameworkStores<CampContext>();

            services.ConfigureApplicationCookie(opts =>
            {
                opts.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        {
                            ctx.Response.StatusCode = 401;
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        {
                            ctx.Response.StatusCode = 403;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddScoped<SignInManager<CampUser>, SignInManager<CampUser>>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env
            , CampDbInitializer seeder, CampIdentityInitializer identitySeeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

//            app.UseCors(cfg =>
//            {
////                cfg.AllowAnyHeader()
////                    .AllowAnyMethod()
////                    .WithOrigins();
//            });

            app.UseAuthentication();

            app.UseMvc(
                //     m => { m.MapRoute("MainAPIRoute", "api/{controller}/{action}"); }
            );
            seeder.Seed().Wait();
            identitySeeder.Seed().Wait();
        }
    }
}