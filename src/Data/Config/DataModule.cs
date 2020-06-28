using Autofac;
using PlexRipper.Data.Common.Interfaces;
using System;
using System.Reflection;
using System.Threading.Tasks;

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
                .InstancePerLifetimeScope();

            // Create Database
            // TODO Move the creation of the Database to a better place, unknown where
            var DB = new PlexRipperDbContext(dbContextOptions);

            // Should the Database be deleted and re-created
            var resetDb = Environment.GetEnvironmentVariable("ResetDB");
            if (resetDb != null && resetDb == "true")
            {
                DB.Database.EnsureDeleted();
            }

            // TODO Re-enable Migrate when stable
            // DB.Database.Migrate();
            Task.WaitAll(DB.Database.EnsureCreatedAsync());
        }
    }
}
