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
using PlexRipper.Application.Common.Mappings;
using PlexRipper.Application.Config;
using PlexRipper.Infrastructure.Common.Mappings;
using PlexRipper.Infrastructure.Config;
using PlexRipper.Infrastructure.Persistence;
using PlexRipper.WebAPI.Config;
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
        public void Configure(IApplicationBuilder app)
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
            // services.AddInfrastructure(Configuration);

            services.AddHttpContextAccessor();

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


            //Setup Database
            var DB = new PlexRipperDbContext(PlexRipperDbContext.GetConfig().Options);
            // TODO Re-enable Migrate when stable
            // DB.Database.Migrate();
            DB.Database.EnsureCreated();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Application
            builder.RegisterModule<ApplicationModule>();

            // Infrastructure
            builder.RegisterModule<InfrastructureModule>();
            builder.RegisterLogger();

            // Auto Mapper
            builder.Register(ctx =>
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new ApplicationMappingProfile());
                    cfg.AddProfile(new InfrastructureMappingProfile());
                });
                config.AssertConfigurationIsValid();
                return config;
            });

            builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>().InstancePerLifetimeScope();
        }
    }
}
