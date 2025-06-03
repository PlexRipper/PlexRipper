using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class SyncPlexLibraryMediaMetaDataCommandHandlerUnitTests
    : BaseUnitTest<SyncPlexLibraryMediaMetaDataCommandHandler>
{
    public SyncPlexLibraryMediaMetaDataCommandHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSyncAllRolesGenresCountries_WhenNoneExist()
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
        var roles = FakeData.GetPlexRoles(seed).Generate(100);
        var genres = FakeData.GetPlexGenres(seed).Generate(100);
        var countries = FakeData.GetPlexCountries(seed).Generate(100);

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = roles,
                Genres = genres,
                Countries = countries,
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var rolesDb = await IDbContext.PlexRoles.ToListAsync();
        rolesDb.Count.ShouldBe(roles.Count);
        foreach (var role in roles)
            rolesDb.ShouldContain(x => x.Name == role.Name && x.PlexKey == role.PlexKey);

        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        genresDb.Count.ShouldBe(genres.Count);
        foreach (var genre in genres)
            genresDb.ShouldContain(x => x.Name == genre.Name && x.PlexKey == genre.PlexKey);

        var countriesDb = await IDbContext.PlexCountries.ToListAsync();
        countriesDb.Count.ShouldBe(countries.Count);
        foreach (var country in countries)
            countriesDb.ShouldContain(x => x.Name == country.Name && x.PlexKey == country.PlexKey);
    }
}
