using System;
using AutoMapper;
using DevnotMentor.Api.Entities;
using DevnotMentor.Api.Helpers;
using DevnotMentor.Api.Services;
using DevnotMentor.Api.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DevnotMentor.Api.ActionFilters;
using DevnotMentor.Api.Utilities.Security.Token;
using DevnotMentor.Api.Configuration.Context;
using DevnotMentor.Api.Configuration.Environment;
using DevnotMentor.Api.Middlewares;
using DevnotMentor.Api.Utilities.Security.Hash;
using DevnotMentor.Api.Utilities.Security.Hash.Sha256;
using DevnotMentor.Api.Utilities.Email;
using DevnotMentor.Api.Repositories;
using DevnotMentor.Api.Repositories.Interfaces;
using DevnotMentor.Api.Utilities.Email.SmtpMail;
using DevnotMentor.Api.Utilities.File;
using DevnotMentor.Api.Utilities.File.Local;
using DevnotMentor.Api.Utilities.Security.Token.Jwt;

namespace DevnotMentor.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = EnvironmentService.StaticConfiguration["ConnectionStrings:SQLServerConnectionString"];
            services.AddDbContext<MentorDBContext>(options => options.UseSqlServer(connectionString));

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            services.AddAutoMapper(typeof(Startup));

            services.AddSingleton<TokenAuthentication>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMentorService, MentorService>();
            services.AddScoped<IMenteeService, MenteeService>();
            services.AddScoped<IPairService, PairService>();
            services.AddScoped<IApplicationService, ApplicationService>();


            services.AddScoped<IMailService, SmtpMailService>();
            services.AddScoped<IFileService, LocalFileService>();

            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddSingleton<IHashService, Sha256HashService>();
            services.AddSingleton<IDevnotConfigurationContext, DevnotConfigurationContext>();
            services.AddSingleton<IEnvironmentService, EnvironmentService>();

            #region Repositories

            services.AddScoped<ILoggerRepository, LoggerRepository>();
            services.AddScoped<IMenteeLinksRepository, MenteeLinksRepository>();
            services.AddScoped<IMenteeRepository, MenteeRepository>();
            services.AddScoped<IMenteeTagsRepository, MenteeTagsRepository>();
            services.AddScoped<IMentorApplicationsRepository, MentorApplicationsRepository>();
            services.AddScoped<IMentorLinksRepository, MentorLinksRepository>();
            services.AddScoped<IMentorMenteePairsRepository, MentorMenteePairsRepository>();
            services.AddScoped<IMentorRepository, MentorRepository>();
            services.AddScoped<IMentorTagsRepository, MentorTagsRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            #endregion

            services.AddCors(options =>
            {
                options
                .AddPolicy("AllowMyOrigin", builder =>
                    builder
                    //.WithOrigins("http://localhost:8080")
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ValidateModelStateAttribute));
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

            services.AddCustomSwagger();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseCors("AllowMyOrigin");
            app.UseCustomSwagger();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
