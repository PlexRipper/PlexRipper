using Autofac;
using AutofacSerilogIntegration;
using AutoMapper;
using Carter;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors.Security;
using PlexRipper.Application;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Services;
using PlexRipper.Infrastructure;
using PlexRipper.Infrastructure.Config;
using PlexRipper.WebAPI.Config;
using PlexRipper.WebAPI.Services;
using System;
using System.Linq;
using System.Reflection;

namespace PlexRipper.WebAPI
{
    public class Startup
    {


        public IConfiguration Configuration { get; private set; }

        public Startup(IWebHostEnvironment env)
        {
            // Setup AutoFac
            Configuration = ContainerConfig.Setup(env);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // TODO Make sure to configure this correctly when setting up security
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            // app.UseHttpsRedirection();

            app.UseOpenApi(); // serve OpenAPI/Swagger documents
            app.UseSwaggerUi3(); // serve Swagger UI

            app.UseRouting();
            app.UseEndpoints(builder => builder.MapCarter());
            // app.UseAuthorization();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplication();
            services.AddInfrastructure(Configuration);


            services.AddHttpContextAccessor();

            // Auto Mapper
            IMapper mapper = SetupAutoMapper();
            services.AddSingleton(mapper);

            // Fluent Validator
            services.AddMvc().AddFluentValidation();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Carter
            services.AddCarter();

            services.AddControllers(); // TODO Might be removed
            services.AddCors();
            // Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddOpenApiDocument(configure =>
            {
                configure.Title = "PlexRipper API";
                configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}."
                });

                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

            // Autofac
            services.AddOptions();
        }

        /// <summary>
        /// Scans all assemblies and adds any found Automapper profiles to the mapper
        /// </summary>
        /// <returns>Returns the mapper object with all profiles added</returns>
        private IMapper SetupAutoMapper()
        {
            var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic);
            var allTypes = assembliesToScan.SelectMany(a => a.GetExportedTypes()).ToArray();

            var profiles =
                allTypes
                    .Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                    .Where(t => !t.GetTypeInfo().IsAbstract);

            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(cfg =>
            {
                foreach (var profile in profiles)
                {
                    cfg.AddProfile(profile);
                }
            });

            return mappingConfig.CreateMapper();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {

            // Application
            builder.RegisterType<PlexService>().As<IPlexService>().SingleInstance();
            builder.RegisterType<AccountService>().As<IAccountService>().InstancePerLifetimeScope();

            // Register Modules
            builder.RegisterModule<InfrastructureModule>();
            builder.RegisterLogger();

            //WebApi
            builder.RegisterType<CurrentUserService>().As<ICurrentUserService>(); //TODO might be removed
        }
    }
}
