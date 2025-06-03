using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class InsertMediaMetaDataCommandUnitTests : BaseCommandUnitTest<InsertMediaMetaDataCommand>
{
    public InsertMediaMetaDataCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSyncAllActorsGenresCountries_WhenNoneExist()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();
        var actors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(100);
        var genres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).Generate(100);
        var countries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).Generate(100);

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Actors = actors,
                Genres = genres,
                Countries = countries,
                Library = plexLibrary,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(actors.Count);
        foreach (var actor in actors)
            actorsDb.ShouldContain(
                x => x.Name == actor.Name && x.Key == actor.TagKey,
                $"Actor {actor.Name} with key {actor.TagKey} not found in database"
            );

        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        genresDb.Count.ShouldBe(59);
        foreach (var genre in genres)
            genresDb.ShouldContain(x => x.Key == genre.Key, $"Genre {genre.Name} not found in database");

        var countriesDb = await IDbContext.PlexCountries.ToListAsync();
        countriesDb.Count.ShouldBe(81);
        foreach (var country in countries)
            countriesDb.ShouldContain(x => x.Key == country.Key, $"Country {country.Name} not found in database");
    }

    [Fact]
    public async Task ShouldUpdateExistingActorsGenresCountries_WhenSomeExist()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create initial data
        var initialActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(50, x => x.TagKey);
        var initialGenres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).GenerateUnique(50, x => x.Key);
        var initialCountries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).GenerateUnique(50, x => x.Key);

        // Insert initial data
        var dbContext = IDbContext;
        await dbContext.PlexActors.AddRangeAsync(initialActors.ToPlexActor());
        await dbContext.PlexGenres.AddRangeAsync(initialGenres.ToPlexGenre());
        await dbContext.PlexCountries.AddRangeAsync(initialCountries.ToPlexCountry());
        await dbContext.SaveChangesAsync();

        // Create new data with some overlaps
        var newActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(100, x => x.TagKey);
        var newGenres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).GenerateUnique(100, x => x.Key);
        var newCountries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).GenerateUnique(100, x => x.Key);

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Actors = newActors,
                Genres = newGenres,
                Countries = newCountries,
                Library = plexLibrary,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(initialActors.Count + newActors.Count);
        foreach (var actor in newActors)
            actorsDb.ShouldContain(x => x.Name == actor.Name && x.Key == actor.TagKey);

        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        genresDb.Count.ShouldBe(newGenres.Count);
        foreach (var genre in newGenres)
            genresDb.ShouldContain(x => x.Key == genre.Key);

        var countriesDb = await IDbContext.PlexCountries.ToListAsync();
        countriesDb.Count.ShouldBe(130);
        foreach (var country in newCountries)
            countriesDb.ShouldContain(x => x.Key == country.Key);
    }

    [Fact]
    public async Task ShouldHandleEmptyLists_WhenNoDataProvided()
    {
        // Arrange
        await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Actors = [],
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.ShouldBeEmpty();

        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        genresDb.ShouldBeEmpty();

        var countriesDb = await IDbContext.PlexCountries.ToListAsync();
        countriesDb.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldHandleDuplicatePlexKeys_WhenDataContainsDuplicates()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create data with duplicate PlexKeys
        var plexApiActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(10);
        var duplicateActors = plexApiActors.Select(r => r with { Name = r.Name + "_duplicate" }).ToList();
        plexApiActors.AddRange(duplicateActors);

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Actors = plexApiActors,
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(10); // Should only have unique PlexKeys
        actorsDb.Select(x => x.Key).Distinct().Count().ShouldBe(10);
    }

    [Fact]
    public async Task ShouldHandleConcurrentSyncs_WhenMultipleOperationsRun()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        var actors1 = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(50);
        var actors2 = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(50);

        // Act
        var command1 = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Actors = actors1,
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            }
        );

        var command2 = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Actors = actors2,
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            }
        );

        var results = await Task.WhenAll(
            TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command1),
            TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command2)
        );

        // Assert
        results.All(x => x.IsSuccess).ShouldBeTrue();
        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(100);
        actorsDb.Select(x => x.Key).Distinct().Count().ShouldBe(100);
    }

    [Fact]
    public async Task ShouldHandleLargeDataSet_WhenManyItemsProvided()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create a large dataset
        var actors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(1000);
        var genres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).Generate(1000);
        var countries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).Generate(1000);

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Actors = actors,
                Genres = genres,
                Countries = countries,
                Library = plexLibrary,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        var countriesDb = await IDbContext.PlexCountries.ToListAsync();

        actorsDb.Count.ShouldBe(1000);
        genresDb.Count.ShouldBe(100); // These are deduplicated
        countriesDb.Count.ShouldBe(239); // These are deduplicated, possible number of countries
    }

    // TODO create a test which checks for actors having a tagkey that is null

    [Fact]
    public async Task ShouldPreserveDataIntegrity_WhenUpdatingExistingItems()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create initial data
        var initialActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(50);
        await IDbContext.PlexActors.AddRangeAsync(initialActors.ToPlexActor());
        await IDbContext.SaveChangesAsync();

        // Create new data with some overlapping PlexKeys but different names
        var newActors = initialActors.Take(25).Select(r => r with { Name = r.Name + "_updated" }).ToList();
        newActors.AddRange(FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(25));

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Actors = newActors,
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(50);

        // Verify that existing items were updated
        foreach (var actor in newActors.Take(25))
        {
            var dbActor = actorsDb.FirstOrDefault(x => x.Key == actor.TagKey);
            dbActor.ShouldNotBeNull();
            dbActor.Name.ShouldBe(actor.Name);
        }
    }

    [Fact]
    public async Task ShouldHandleNullLibraryMetadata_WhenProvided()
    {
        // Arrange
        await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Act
        var command = new InsertMediaMetaDataCommand(LibraryMetadata: null!);
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Has400BadRequestError().ShouldBeTrue();
    }
}
