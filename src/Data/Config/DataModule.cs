using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.Config
{
    public class DataModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // register all I*Repository
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces();

            // Setup Database
            var dbContextOptions = PlexRipperDbContext.GetConfig().Options;

            var testMode = Environment.GetEnvironmentVariable("IntegrationTestMode");
            if (testMode != null && testMode == "true")
            {
                // Register Entity Framework Database in TestMode for IntegrationTests
                dbContextOptions = PlexRipperDbContext.GetTestConfig().Options;
            }

            builder.RegisterType<PlexRipperDbContext>()
                .WithParameter("options", dbContextOptions)
                .As<IPlexRipperDbContext>()
                .InstancePerDependency(); // TODO this might need to be InstancePerLifetime
        }
    }
}