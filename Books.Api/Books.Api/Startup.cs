using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Books.Api.Contexts;
using Books.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Books.Api
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // register DBContext on the container, getting the connection string from
            // appSettings (note: use this during development, in a production enviroment,
            // it is better to store the connection string in an enviroment variable)
            var connectionString = Configuration["ConnectionStrings:BooksDBConnectionString"];
            services.AddDbContext<BooksContext>(o => o.UseSqlServer(connectionString));

            // should be registered with "Scoped", Or "Transient", Or "SingletonLifeTime"?
            // in our case the "repository" is a services that uses "BooksContext", and the "BookContext" is a "DbContext"
            // which means we must register it with a scope that is equal to or shorter than the "DbContext" Scope.
            // "Singleton" will have life time larger than the "DbContext" scope, and in that case the "BookContext" will have Uncorrect state when processing subsequent requests.
            // "Transient" scope, with "Transient" in each request for a service we will have a new instance served up, which means we will lose any state our repository might hold if it is requested by multiple parts of our code.
            // Note: EFCore dispose DbContext after every request, for "Singleton" scope, all records that were once read across requests would be tracked by "DbContext" by default, and that means that eventually performance would get lower when 
            // more and  more entities would be tracked
            services.AddScoped<IBooksRepository, BooksRepository>();

            services.AddAutoMapper(typeof(Startup));

            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
