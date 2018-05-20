using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCodeCamp.Data;
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
            services.AddCors(cfg =>
            {
                cfg.AddPolicy("Wildermuth", bldr =>
                {
                    bldr.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("htttp://wildermuth.com");
                });

                cfg.AddPolicy("AnyGET", bldr =>
                {
                    bldr.AllowAnyHeader()
                        .WithMethods("GET")
                        .AllowAnyOrigin();
                });
            });
            services.AddMvc(opt =>
                    {
                        if( !_env.IsProduction()) opt.SslPort = 44380;
                        opt.Filters.Add(new RequireHttpsAttribute());
                    }
                    
                    )
                
                .AddJsonOptions(opt => { opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; });

            services.AddScoped<ICampRepository, CampRepository>();
            services.AddTransient<CampDbInitializer>();

            services.AddAutoMapper();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env
        ,CampDbInitializer seeder)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseCors(cfg =>
            {
//                cfg.AllowAnyHeader()
//                    .AllowAnyMethod()
//                    .WithOrigins();
                
            });
            app.UseMvc(
                //     m => { m.MapRoute("MainAPIRoute", "api/{controller}/{action}"); }
            );
            seeder.Seed().Wait();
        }
    }
}