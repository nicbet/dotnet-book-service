using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NSwag.AspNetCore;

namespace BookApi
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
            // Database Configuration
            string server = Configuration["MSSQL_SERVER"] ?? "localhost";
            string password = Configuration["MSSQL_PASSWORD"] ?? "Passw0rd!";
            string user = Configuration["MSSQL_USER"] ?? "sa";
            string dbName = Configuration["MSSQL_DB_NAME"] ?? "books";
            string connectionString = $@"Server={server};Database={dbName};User Id={user};Password={password};";
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDbContext<BookContext>(options =>
                options.UseSqlServer(connectionString)
            );

            // Swagger API Pages
            services.AddSwagger();

            // Redis Caching
            services.AddDistributedRedisCache(options =>
                {
                    options.Configuration = Configuration["REDIS_SERVER"] ?? "127.0.0.1";
                    options.InstanceName = Configuration["REDIS_INSTANCE_NAME"] ?? "master";
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, BookContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Automatically apply all migrations
            context.Database.Migrate();

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseSwaggerUi3WithApiExplorer(settings =>
            {
                settings.GeneratorSettings.DefaultPropertyNameHandling =
                PropertyNameHandling.CamelCase;

                settings.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "Book Services Services API";
                    document.Info.Description = "Data access service for books";
                    document.Info.TermsOfService = "None";
                    document.Info.Contact = new NSwag.SwaggerContact
                    {
                        Name = "Nicolas Bettenburg",
                        Email = "nicbet@gmail.com",
                    };
                    document.Schemes.Add(NSwag.SwaggerSchema.Http);
                    document.Schemes.Add(NSwag.SwaggerSchema.Https);
                };
            });
        }
    }
}
