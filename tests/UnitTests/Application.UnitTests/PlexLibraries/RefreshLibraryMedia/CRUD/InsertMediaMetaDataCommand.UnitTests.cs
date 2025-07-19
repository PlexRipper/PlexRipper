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
            LibraryMetadata: new LibraryMetadata(plexLibrary)
            {
                Actors = actors,
                Genres = genres,
                Countries = countries,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(actors.Count);
        foreach (var actor in actors)
            actorsDb.ShouldContain(
                x => x.Name == actor.Name && x.Key == actor.Key,
                $"Actor {actor.Name} with key {actor.Key} not found in database"
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
        var initialActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(50, x => x.Key);
        var initialGenres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).GenerateUnique(50, x => x.Key);
        var initialCountries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).GenerateUnique(50, x => x.Key);

        // Insert initial data
        var dbContext = IDbContext;
        await dbContext.PlexActors.AddRangeAsync(initialActors.ToPlexActor());
        await dbContext.PlexGenres.AddRangeAsync(initialGenres.ToPlexGenre());
        await dbContext.PlexCountries.AddRangeAsync(initialCountries.ToPlexCountry());
        await dbContext.SaveChangesAsync();

        // Create new data with some overlaps
        var newActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(100, x => x.Key);
        var newGenres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).GenerateUnique(100, x => x.Key);
        var newCountries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).GenerateUnique(100, x => x.Key);

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary)
            {
                Actors = newActors,
                Genres = newGenres,
                Countries = newCountries,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(initialActors.Count + newActors.Count);
        foreach (var actor in newActors)
            actorsDb.ShouldContain(x => x.Name == actor.Name && x.Key == actor.Key);

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
        var command = new InsertMediaMetaDataCommand(LibraryMetadata: new LibraryMetadata(plexLibrary));
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
            LibraryMetadata: new LibraryMetadata(plexLibrary) { Actors = plexApiActors }
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

        var actors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(100, x => x.Name);

        // Act
        var command1 = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary) { Actors = actors.Slice(0, 50) }
        );

        var command2 = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary) { Actors = actors.Slice(50, 50) }
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
        var actors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(1000, x => x.Name);
        var genres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).Generate(1000);
        var countries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).Generate(1000);

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary)
            {
                Actors = actors,
                Genres = genres,
                Countries = countries,
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
            LibraryMetadata: new LibraryMetadata(plexLibrary) { Actors = newActors }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(50);

        // Verify that existing items were updated
        foreach (var actor in newActors.Take(25))
        {
            var dbActor = actorsDb.FirstOrDefault(x => x.Key == actor.Key);
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

    [Fact]
    public async Task ShouldFilterOutActorsWithNullTagKey_WhenActorsHaveNullKeys()
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

        // Create actors with some having null TagKey
        var actorsWithKeys = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(50, x => x.Name);
        var actorsWithNullKeys = FakePlexApiData
            .GetLibraryMediaItemActorDTO(seed)
            .GenerateUnique(30, x => x.Name)
            .Select(x => x with { Key = string.Empty })
            .ToList();

        var allActors = actorsWithKeys.Concat(actorsWithNullKeys).ToList();

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary) { Actors = allActors }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var actorsDb = await IDbContext.PlexActors.ToListAsync();

        // Only actors with Tag should be inserted
        actorsDb.Count.ShouldBe(50);
        actorsDb.ShouldAllBe(x => x.Key != string.Empty);
    }

    [Fact]
    public async Task ShouldHandlePartiallyEmptyMetadata_WhenOnlySomeListsAreEmpty()
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

        var actors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(25, x => x.Name);

        // Genres and Countries are empty

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary) { Actors = actors }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.PlexActors.Count.ShouldBe(25);
        result.Value.PlexGenres.ShouldBeEmpty();
        result.Value.PlexCountries.ShouldBeEmpty();

        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        var countriesDb = await IDbContext.PlexCountries.ToListAsync();

        actorsDb.Count.ShouldBe(25);
        genresDb.ShouldBeEmpty();
        countriesDb.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnCorrectDictionaryMappings_WhenDataIsInserted()
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

        var actors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(10, x => x.Key);
        var genres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).GenerateUnique(10, x => x.Key);
        var countries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).GenerateUnique(10, x => x.Key);

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary)
            {
                Actors = actors,
                Genres = genres,
                Countries = countries,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify dictionary keys match the source PlexIds
        foreach (var actor in actors.Where(x => x.Key != null))
        {
            result.Value.PlexActors.ShouldContainKey(actor.Key);
            result.Value.PlexActors[actor.Key].Name.ShouldBe(actor.Name);
        }

        foreach (var genre in genres)
        {
            result.Value.PlexGenres.ShouldContainKey(genre.Key);
            result.Value.PlexGenres[genre.Key].Key.ShouldBe(genre.Key);
        }

        foreach (var country in countries)
        {
            result.Value.PlexCountries.ShouldContainKey(country.Key);
            result.Value.PlexCountries[country.Key].Key.ShouldBe(country.Key);
        }
    }

    [Fact]
    public async Task ShouldHandleSpecialCharactersInNames_WhenDataContainsUnicodeCharacters()
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

        // Create actors with special characters
        var baseActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(5);
        var specialActors = baseActors
            .Select(
                (actor, index) =>
                    actor with
                    {
                        Name = index switch
                        {
                            0 => "José María Aznar",
                            1 => "André François",
                            2 => "张三丰",
                            3 => "محمد عبدالله",
                            4 => "Björk Guðmundsdóttir",
                            _ => actor.Name,
                        },
                    }
            )
            .ToList();

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary) { Actors = specialActors }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(5);

        actorsDb.ShouldContain(x => x.Name == "José María Aznar");
        actorsDb.ShouldContain(x => x.Name == "André François");
        actorsDb.ShouldContain(x => x.Name == "张三丰");
        actorsDb.ShouldContain(x => x.Name == "محمد عبدالله");
        actorsDb.ShouldContain(x => x.Name == "Björk Guðmundsdóttir");
    }

    [Fact]
    public async Task ShouldMaintainConsistency_WhenSameDataInsertedMultipleTimes()
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

        var actors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(20);
        var genres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).Generate(20);
        var countries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).Generate(20);

        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary)
            {
                Actors = actors,
                Genres = genres,
                Countries = countries,
            }
        );

        // Act - Insert the same data multiple times
        var result1 = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);
        var result2 = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);
        var result3 = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        result3.IsSuccess.ShouldBeTrue();

        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        var countriesDb = await IDbContext.PlexCountries.ToListAsync();

        // Should not have duplicates
        actorsDb.Count.ShouldBe(20);
        actorsDb.Select(x => x.Key).Distinct().Count().ShouldBe(20);

        genresDb.Select(x => x.Key).Distinct().Count().ShouldBe(genresDb.Count);
        countriesDb.Select(x => x.Key).Distinct().Count().ShouldBe(countriesDb.Count);
    }

    [Fact]
    public async Task ShouldCorrectlyMapLibraryReference_WhenLibraryIsProvided()
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

        var actors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(5);

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary) { Actors = actors }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.PlexLibrary.ShouldBe(plexLibrary);
        result.Value.PlexLibraryId.ShouldBe(plexLibrary.Id);
    }

    [Fact]
    public async Task ShouldHandleVeryLongNames_WhenDataContainsLongStrings()
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

        // Create actors with very long names
        var baseActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(3);
        var longNameActors = baseActors
            .Select(
                (actor, index) =>
                    actor with
                    {
                        // Very long name
                        Name = new string('A', 250) + index,
                    }
            )
            .ToList();

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary) { Actors = longNameActors }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        actorsDb.Count.ShouldBe(3);
        actorsDb.ShouldAllBe(x => x.Name.Length >= 250);
    }

    [Fact]
    public async Task ShouldReturnEmptyDictionaries_WhenAllListsAreEmpty()
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
        var command = new InsertMediaMetaDataCommand(LibraryMetadata: new LibraryMetadata(plexLibrary));
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.PlexActors.ShouldBeEmpty();
        result.Value.PlexGenres.ShouldBeEmpty();
        result.Value.PlexCountries.ShouldBeEmpty();
        result.Value.PlexLibrary.ShouldBe(plexLibrary);
    }

    [Fact]
    public async Task ShouldHandleZeroKeys_WhenDataContainsZeroValues()
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

        // Create data with zero keys
        var baseData = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).Generate(5);
        var actorsWithZeroKeys = baseData.Select(x => x with { Key = string.Empty }).ToList();

        var baseGenres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).Generate(3);
        var genresWithZeroKeys = baseGenres.Select(x => x with { Key = string.Empty }).ToList();

        var baseCountries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).Generate(3);
        var countriesWithZeroKeys = baseCountries.Select(x => x with { Key = string.Empty }).ToList();

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary)
            {
                Actors = actorsWithZeroKeys,
                Genres = genresWithZeroKeys,
                Countries = countriesWithZeroKeys,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        var countriesDb = await IDbContext.PlexCountries.ToListAsync();

        actorsDb.Count.ShouldBe(0);
        genresDb.Count.ShouldBe(0);
        countriesDb.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldReturnValidDatabaseIds_WhenEntitiesAreInserted()
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

        var actors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(15, x => x.Key);
        var genres = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).GenerateUnique(12, x => x.Key);
        var countries = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).GenerateUnique(8, x => x.Key);

        // Act
        var command = new InsertMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata(plexLibrary)
            {
                Actors = actors,
                Genres = genres,
                Countries = countries,
            }
        );
        var result = await TestHandlerExecuteAsync<InsertMediaMetaDataCommandResponse>(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify dictionary keys are valid PlexIds
        result.Value.PlexActors.Keys.ShouldAllBe(
            key => !string.IsNullOrEmpty(key),
            "All actor PlexIds should be positive"
        );
        result.Value.PlexGenres.Keys.ShouldAllBe(
            key => !string.IsNullOrEmpty(key),
            "All genre PlexIds should be positive"
        );
        result.Value.PlexCountries.Keys.ShouldAllBe(
            key => !string.IsNullOrEmpty(key),
            "All country PlexIds should be positive"
        );

        // Verify entities have valid database IDs
        result.Value.PlexActors.Values.ShouldAllBe(actor => actor.Id > 0, "All actors should have valid database IDs");
        result.Value.PlexGenres.Values.ShouldAllBe(genre => genre.Id > 0, "All genres should have valid database IDs");
        result.Value.PlexCountries.Values.ShouldAllBe(
            country => country.Id > 0,
            "All countries should have valid database IDs"
        );

        // Verify database entities match returned entities
        var actorsDb = await IDbContext.PlexActors.ToListAsync();
        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        var countriesDb = await IDbContext.PlexCountries.ToListAsync();

        actorsDb.Count.ShouldBe(result.Value.PlexActors.Count);
        genresDb.Count.ShouldBe(result.Value.PlexGenres.Count);
        countriesDb.Count.ShouldBe(result.Value.PlexCountries.Count);

        // Verify each returned entity exists in the database
        foreach (var returnedActor in result.Value.PlexActors.Values)
        {
            var dbActor = actorsDb.FirstOrDefault(x => x.Id == returnedActor.Id);
            dbActor.ShouldNotBeNull($"Actor with ID {returnedActor.Id} should exist in database");
            dbActor.Name.ShouldBe(returnedActor.Name);
            dbActor.Key.ShouldBe(returnedActor.Key);
        }

        foreach (var returnedGenre in result.Value.PlexGenres.Values)
        {
            var dbGenre = genresDb.FirstOrDefault(x => x.Id == returnedGenre.Id);
            dbGenre.ShouldNotBeNull($"Genre with ID {returnedGenre.Id} should exist in database");
            dbGenre.Name.ShouldBe(returnedGenre.Name);
            dbGenre.Key.ShouldBe(returnedGenre.Key);
        }

        foreach (var returnedCountry in result.Value.PlexCountries.Values)
        {
            var dbCountry = countriesDb.FirstOrDefault(x => x.Id == returnedCountry.Id);
            dbCountry.ShouldNotBeNull($"Country with ID {returnedCountry.Id} should exist in database");
            dbCountry.Name.ShouldBe(returnedCountry.Name);
            dbCountry.Key.ShouldBe(returnedCountry.Key);
        }

        // Verify source data mapping is correct
        foreach (var sourceActor in actors.Where(x => !string.IsNullOrEmpty(x.Key)))
        {
            result.Value.PlexActors.ShouldContainKey(
                sourceActor.Key,
                $"Response should contain actor with PlexId {sourceActor.Key}"
            );
            var responseActor = result.Value.PlexActors[sourceActor.Key];
            responseActor.Name.ShouldBe(sourceActor.Name);
        }

        foreach (var sourceGenre in genres.Where(x => !string.IsNullOrEmpty(x.Key)))
        {
            result.Value.PlexGenres.ShouldContainKey(
                sourceGenre.Key,
                $"Response should contain genre with PlexId {sourceGenre.Key}"
            );
            var responseGenre = result.Value.PlexGenres[sourceGenre.Key];
            responseGenre.Name.ShouldBe(sourceGenre.Name);
        }

        foreach (var sourceCountry in countries.Where(x => !string.IsNullOrEmpty(x.Key)))
        {
            result.Value.PlexCountries.ShouldContainKey(
                sourceCountry.Key,
                $"Response should contain country with PlexId {sourceCountry.Key}"
            );
            var responseCountry = result.Value.PlexCountries[sourceCountry.Key];
            responseCountry.Name.ShouldBe(sourceCountry.Name);
        }
    }
}
