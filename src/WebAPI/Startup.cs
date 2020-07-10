using Autofac;
using AutofacSerilogIntegration;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors.Security;
using PlexRipper.Application.Config;
using PlexRipper.SignalR.Hubs;
using PlexRipper.WebAPI.Config;
using System.Linq;
using System.Reflection;

namespace PlexRipper.WebAPI
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; private set; }

        public ILifetimeScope AutofacContainer { get; private set; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app)
        {

            // TODO Make sure to configure this correctly when setting up security
            app.UseCors(builder => builder
                .WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
            );

            // app.UseHttpsRedirection();

            app.UseOpenApi(); // serve OpenAPI/Swagger documents
            app.UseSwaggerUi3(); // serve Swagger UI

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<DownloadProgressHub>("/download/progress");
            });

            // app.UseAuthorization();
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // General
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddCors();

            // Fluent Validator
            services.AddMvc().AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssemblyContaining<ApplicationModule>();
                fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
            });
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // SignalR
            services.AddSignalR();

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

        public void ConfigureContainer(ContainerBuilder builder)
        {
            ContainerConfig.ConfigureContainer(builder);
            builder.RegisterLogger(autowireProperties: true);

        }

    }
}
