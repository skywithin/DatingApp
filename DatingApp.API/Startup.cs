using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<DataContext>(
            //    x =>
            //        x.UseSqlite(
            //            Configuration.GetConnectionString("DefaultConnection")));

            // Use in-memony database
            services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("InMemoryTestDb"));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddCors();

            services.AddScoped<IAuthRepository, AuthRepository>();

            services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(
                    options =>
                    {
                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey =
                                    new SymmetricSecurityKey(
                                        Encoding.ASCII.GetBytes(
                                            Configuration.GetSection("AppSettings:Token").Value)),
                                ValidateIssuer = false,
                                ValidateAudience = false
                            };
                    });
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
                app.UseExceptionHandler(
                    builder => {
                        builder.Run(
                            async context => {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                                var error = context.Features.Get<IExceptionHandlerFeature>();
                                if (error != null)
                                {
                                    context.Response.AddApplicationError(error.Error.Message);
                                    await context.Response.WriteAsync(error.Error.Message);
                                }
                            });
                    });
            }

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DataContext>();
                SeedData(context);
            }

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();

            app.UseMvc();
        }



        /// <summary>
        /// Populate initial values for in-memory database
        /// </summary>
        /// <param name="context"></param>
        private static void SeedData(DataContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            // Populate "User" table
            var users = new User[]
            {
                new User {
                    Id = 1,
                    Username = "max",
                    PasswordHash = new byte[] { 148,58,151,116,58,18,250,202,173,210,8,95,239,247,218,50,20,56,95,3,102,245,152,62,157,188,124,140,43,171,188,188,203,158,107,223,182,170,35,214,172,157,111,220,8,236,98,217,80,92,195,252,44,118,0,251,156,100,112,204,248,193,126,226 },
                    PasswordSalt = new byte[] { 241,183,211,31,135,58,50,160,253,245,248,151,173,128,2,107,1,89,37,138,144,178,168,245,174,8,237,224,233,92,191,147,176,77,224,95,202,180,182,57,33,94,183,27,121,249,175,41,230,165,204,235,255,117,32,91,148,123,235,87,14,45,139,15,238,16,113,231,151,245,117,199,218,164,128,238,169,101,29,74,39,161,229,156,162,7,150,6,180,8,78,43,42,243,92,166,84,92,236,157,216,33,62,166,171,165,207,233,118,110,125,63,87,67,252,97,25,53,145,140,237,197,222,84,31,165,65,210 }
                },
            };

            foreach (var u in users)
            {
                context.Users.Add(u);
            }

            context.SaveChanges();

            // Populate "Value" table
            var values = new Value[]
            {
                new Value
                {
                    Id = 1,
                    Name = "Value 101"
                },
                new Value
                {
                    Id = 2,
                    Name = "Value 102"
                },
                new Value
                {
                    Id = 3,
                    Name = "Value 103"
                },
            };

            foreach (var v in values)
            {
                context.Values.Add(v);
            }

            context.SaveChanges();
        }
    }
}
