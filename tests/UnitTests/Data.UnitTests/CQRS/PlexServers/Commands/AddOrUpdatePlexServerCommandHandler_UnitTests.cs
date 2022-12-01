using PlexRipper.Application;
using PlexRipper.Data.PlexServers;

namespace Data.UnitTests.PlexServers.Commands;

public class AddOrUpdatePlexServerCommandHandler_UnitTests : BaseUnitTest<AddOrUpdatePlexServerCommandHandler_UnitTests>
{
    public AddOrUpdatePlexServerCommandHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Should_When()
    {
        // Arrange
        Action<UnitTestDataConfig> config = dataConfig =>
        {
            dataConfig.Seed = 456324;
            dataConfig.MovieCount = 5;
        };
        var plexAccount = FakeData.GetPlexAccount(config).Generate(1);
        var plexServers = FakeData.GetPlexServer(config).Generate(5);
        await using var context = MockDatabase.GetMemoryDbContext(disableForeignKeyCheck: true);
        var request = new AddOrUpdatePlexServersCommand(plexServers);
        var handler = new AddOrUpdatePlexServersCommandHandler(context);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}