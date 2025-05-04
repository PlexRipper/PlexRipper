using Data.Contracts;
using EFCore.BulkExtensions;
using Logging.Interface;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;
using PlexRipper.Data;
using PlexRipper.Identity;

namespace PlexRipper.BaseTests;

public static partial class MockDatabase
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(MockDatabase));
    private static readonly NaturalSortComparer NaturalComparer = new(StringComparison.InvariantCultureIgnoreCase);

    #region Methods

    #region Private

    private static async Task<PlexRipperDbContext> AddPlexServers(
        this PlexRipperDbContext context,
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        if (!config.ShouldHavePlexServer)
            return context;

        // Generate fake servers
        for (var i = 0; i < config.PlexServerCount; i++)
        {
            var plexServer = FakeData.GetPlexServer(seed, options).Generate();
            context.PlexServers.Add(plexServer);
        }

        await context.SaveChangesAsync();
        var plexServers = await context.PlexServers.ToListAsync();

        // Add Connection to each server
        foreach (var plexServer in plexServers)
        {
            var connections = FakeData
                .GetPlexServerConnections(seed, plexServerId: plexServer.Id)
                .Generate(config.PlexServerConnectionPerServerCount);
            context.PlexServerConnections.AddRange(connections);
        }

        await context.SaveChangesAsync();
        var plexConnections = await context.PlexServerConnections.ToListAsync();

        // Add status to each connection
        foreach (var connection in plexConnections)
        {
            var status = FakeData
                .GetPlexServerStatus(seed, plexServerId: connection.PlexServerId, plexServerConnectionId: connection.Id)
                .Generate();
            context.PlexServerStatuses.Add(status);
        }

        await context.SaveChangesAsync();

        _log.Here()
            .Debug(
                "Added {PlexServerCount} {NameOfPlexServer}s to {NameOfPlexRipperDbContext}: {DatabaseName}",
                config.PlexServerCount,
                nameof(PlexServer),
                nameof(PlexRipperDbContext),
                context.DatabaseName
            );
        return context;
    }

    private static async Task<PlexRipperDbContext> AddPlexLibraries(
        this PlexRipperDbContext context,
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        if (!config.ShouldHavePlexLibrary)
            return context;

        var plexServers = await context.PlexServers.ToListAsync();
        plexServers.ShouldNotBeEmpty();

        var plexLibrariesToDb = new List<PlexLibrary>();

        foreach (var plexServer in plexServers)
        {
            var plexLibraries = new List<PlexLibrary>();
            if (config.ShouldHaveMoviePlexLibrary)
                plexLibraries.AddRange(
                    FakeData
                        .GetPlexLibrary(seed, PlexMediaType.Movie)
                        .Generate(Math.Max(1, config.PlexMovieLibraryCount))
                );

            if (config.ShouldHaveTvShowPlexLibrary)
                plexLibraries.AddRange(
                    FakeData
                        .GetPlexLibrary(seed, PlexMediaType.TvShow)
                        .Generate(Math.Max(1, config.PlexTvShowLibraryCount))
                );

            foreach (var plexLibrary in plexLibraries)
                plexLibrary.PlexServerId = plexServer.Id;

            plexLibrariesToDb.AddRange(plexLibraries);
        }

        context.PlexLibraries.AddRange(plexLibrariesToDb);
        await context.SaveChangesAsync();
        return context;
    }

    private static async Task<PlexRipperDbContext> AddPlexAccount(
        this PlexRipperDbContext context,
        Seed seed,
        Action<FakeDataConfig>? options
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        var plexServers = context.PlexServers.Include(x => x.PlexLibraries).ToList();

        for (var i = 0; i < config.PlexAccountCount; i++)
        {
            var plexAccount = FakeData.GetPlexAccount(seed).Generate();

            await context.PlexAccounts.AddAsync(plexAccount);
            await context.SaveChangesAsync();

            _log.Here()
                .Debug(
                    "Added 1 {NameOfPlexAccount}: {PlexAccountTitle} to PlexRipperDbContext: {DatabaseName}",
                    nameof(PlexAccount),
                    plexAccount.Title,
                    context.DatabaseName
                );

            var plexAccountServer = plexServers.Select(x => new PlexAccountServer
            {
                AuthTokenCreationDate = DateTime.UtcNow,
                PlexServerId = x.Id,
                PlexAccountId = plexAccount.Id,
                AuthToken = "FAKE_AUTH_TOKEN",
                IsServerOwned = true,
            });

            // Add account -> server relation
            context.PlexAccountServers.AddRange(plexAccountServer);
            await context.SaveChangesAsync();

            // Add account -> library relation
            var plexAccountLibraries = plexServers
                .SelectMany(x => x.PlexLibraries)
                .Select(x => new PlexAccountLibrary
                {
                    PlexAccountId = plexAccount.Id,
                    PlexServerId = x.PlexServerId,
                    PlexLibraryId = x.Id,
                    IsLibraryOwned = true,
                });
            context.PlexAccountLibraries.AddRange(plexAccountLibraries);
        }

        await context.SaveChangesAsync();

        return context;
    }

    private static async Task<PlexRipperDbContext> AddPlexAccountLibraries(this PlexRipperDbContext context)
    {
        var plexLibraries = await context.PlexLibraries.ToListAsync();
        var plexAccounts = await context.PlexAccounts.ToListAsync();
        plexAccounts.ShouldNotBeEmpty();
        plexLibraries.ShouldNotBeEmpty();

        var plexAccountLibraries = new List<PlexAccountLibrary>();
        foreach (var plexAccount in plexAccounts)
        foreach (var plexLibrary in plexLibraries)
            plexAccountLibraries.Add(
                new PlexAccountLibrary
                {
                    PlexAccountId = plexAccount.Id,
                    PlexServerId = plexLibrary.PlexServerId,
                    PlexLibraryId = plexLibrary.Id,
                    IsLibraryOwned = true,
                }
            );

        context.PlexAccountLibraries.AddRange(plexAccountLibraries);
        await context.SaveChangesAsync();
        return context;
    }

    #endregion

    #region Public

    public static string GetMemoryDatabaseName() =>
        $"memory_database_{Random.Shared.Next(1, int.MaxValue)}_{Random.Shared.Next(int.MaxValue)}";

    /// <summary>
    /// Creates an in-memory database only to be used for unit and integration testing.
    /// Passing in the same dbName will create a new context for the same database
    /// </summary>
    /// <param name="dbName">leave empty to generate a random one</param>
    /// <returns>A <see cref="PlexRipperDbContext" /> in memory instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    ///
    public static (PlexRipperDbContext, AuthDbContext) GetMemoryDbContext(string dbName = "") =>
        (GetMemoryPlexRipperDbContext(dbName), GetMemoryAuthDbContext(dbName));

    public static PlexRipperDbContext GetMemoryPlexRipperDbContext(string dbName = "")
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlexRipperDbContext>();
        dbName = string.IsNullOrEmpty(dbName) ? GetMemoryDatabaseName() : dbName;

        SqliteConnection databaseConnection = new(DatabaseConnectionString(dbName));

        databaseConnection.CreateCollation(
            OrderByNaturalExtensions.CollationName,
            (x, y) => NaturalComparer.Compare(x, y)
        );

        optionsBuilder.UseSqlite(databaseConnection);

        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.LogTo(text => LogManager.DbContextLogger(text), LogLevel.Warning);
        return new PlexRipperDbContext(optionsBuilder.Options, dbName);
    }

    public static AuthDbContext GetMemoryAuthDbContext(string dbName = "")
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        dbName = string.IsNullOrEmpty(dbName) ? GetMemoryDatabaseName() : dbName;

        SqliteConnection databaseConnection = new(DatabaseConnectionString(dbName));

        databaseConnection.CreateCollation(
            OrderByNaturalExtensions.CollationName,
            (x, y) => NaturalComparer.Compare(x, y)
        );

        optionsBuilder.UseSqlite(databaseConnection);

        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.LogTo(text => LogManager.DbContextLogger(text), LogLevel.Warning);
        return new AuthDbContext(optionsBuilder.Options, dbName);
    }

    public static string DatabaseConnectionString(string dbName = "") =>
        // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases
        new SqliteConnectionStringBuilder
        {
            // TODO:Should be set to in-memory for testing, flakey tests might be fixed now
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,

            // Database name
            DataSource = dbName,
            Cache = SqliteCacheMode.Shared,
        }.ToString();

    public static async Task Setup(
        this (PlexRipperDbContext, AuthDbContext) context,
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        var (plexRipperContext, authContext) = context;

        authContext.Migrate();

        // PlexServers and Libraries added
        _log.Here()
            .Debug(
                "Setting up {NameOfPlexRipperDbContext} for {DatabaseName}",
                nameof(PlexRipperDbContext),
                plexRipperContext.DatabaseName
            );

        if (config.ShouldHavePlexServer)
            plexRipperContext = await plexRipperContext.AddPlexServers(seed, options);

        if (config.ShouldHavePlexLibrary)
            plexRipperContext = await plexRipperContext.AddPlexLibraries(seed, options);

        if (config.PlexAccountCount > 0)
            plexRipperContext = await plexRipperContext.AddPlexAccount(seed, options);

        if (config.MovieCount > 0)
            plexRipperContext = await plexRipperContext.AddPlexMovies(seed, options);

        if (config.TvShowCount > 0)
            plexRipperContext = await plexRipperContext.AddPlexTvShows(seed, options);

        if (config.MovieDownloadTasksCount > 0)
            plexRipperContext = await plexRipperContext.AddDownloadTaskMovies(seed, options);

        if (config.TvShowDownloadTasksCount > 0)
            plexRipperContext = await plexRipperContext.AddDownloadTaskTvShows(seed, options);

        if (config.AccountHasAccessToAllLibraries)
            plexRipperContext = await plexRipperContext.AddPlexAccountLibraries();

        plexRipperContext.ShouldNotBeNull();
    }

    #endregion

    #endregion

    #region Add Media

    private static async Task<PlexRipperDbContext> AddPlexMovies(
        this PlexRipperDbContext context,
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        var plexLibraries = context.PlexLibraries.Where(x => x.Type == PlexMediaType.Movie).ToList();
        plexLibraries.ShouldNotBeNull().ShouldNotBeEmpty();

        // Add movies for each library
        foreach (var plexLibrary in plexLibraries)
        {
            var movies = FakeData.GetPlexMovies(seed, options).Generate(config.MovieCount);
            await context.BulkInsertPlexMoviesAsync(movies, plexLibrary.PlexServerId, plexLibrary.Id);
        }

        _log.Here()
            .Debug(
                "Added {MovieCount} {NameOfPlexMovie}s to PlexRipperDbContext: {DatabaseName}",
                config.MovieCount,
                nameof(PlexMovie),
                context.DatabaseName
            );

        return context;
    }

    private static async Task<PlexRipperDbContext> AddPlexTvShows(
        this PlexRipperDbContext context,
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);

        var plexLibraries = context.PlexLibraries.Where(x => x.Type == PlexMediaType.TvShow).ToList();
        plexLibraries.ShouldNotBeNull().ShouldNotBeEmpty();

        var tvShowList = new List<PlexTvShow>();
        foreach (var plexLibrary in plexLibraries)
        {
            var tvShows = FakeData.GetPlexTvShows(seed, options).Generate(config.TvShowCount);
            tvShows.SetRelationshipIds(plexLibrary.PlexServerId, plexLibrary.Id);

            tvShowList.AddRange(tvShows);
        }

        await context.BulkInsertPlexTvShowsAsync(tvShowList);

        _log.Here()
            .Debug(
                "Added {TvShowCount} {NameOfPlexTvShow}s to PlexRipperDbContext: {DatabaseName}",
                config.TvShowCount,
                nameof(PlexTvShow),
                context.DatabaseName
            );

        return context;
    }

    #endregion
}
