using Autofac;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using System;
using System.Reflection;
using System.Threading.Tasks;
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

            // Create Database
            // TODO Move the creation of the Database to a better place, unknown where
            var DB = new PlexRipperDbContext(dbContextOptions);

            // Should the Database be deleted and re-created
            var resetDb = Environment.GetEnvironmentVariable("ResetDB");
            if (resetDb != null && resetDb == "true")
            {
                Log.Warning("ResetDB command is true, database will be deleted and re-created.");
                DB.Database.EnsureDeleted();
            }

            // TODO Re-enable Migrate when stable
            // DB.Database.Migrate();
            // TODO This should maybe be setup with Reactive extensions
            Task.Run(async () =>
            {
                var exist = await DB.Database.CanConnectAsync();
                if (!exist)
                {
                    Log.Information("Database does not exist, creating one now.");
                    await DB.Database.EnsureCreatedAsync();
                    exist = await DB.Database.CanConnectAsync();

                    if (exist)
                    {
                        Log.Information("Database was successfully created!");
                    }
                    else
                    {
                        Log.Error("Database could not be created.");
                    }
                }
            });
        }
    }
}