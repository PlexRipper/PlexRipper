using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class RefreshPlexServerAccessCommandUnitTests : BaseUnitTest<RefreshPlexServerAccessCommandHandler>
{
    public RefreshPlexServerAccessCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnOkResult_WhenThereAreNoAccessiblePlexServers()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexAccountCount = 1;
        });

        var plexAccount = await IDbContext.PlexAccounts.FirstOrDefaultAsync();
        plexAccount.ShouldNotBeNull();

        mock.Mock<IPlexApiService>()
            .Setup(x => x.GetAccessiblePlexServersAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PlexServerAccessDTO>());

        // Act
        var request = new RefreshPlexServerAccessCommand(plexAccount.Id);
        var handler = mock.Create<RefreshPlexServerAccessCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldReturnOkResult_WhenThereAreAccessiblePlexServers()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexAccountCount = 1;
        });

        var plexAccount = await IDbContext.PlexAccounts.FirstOrDefaultAsync();
        plexAccount.ShouldNotBeNull();

        var plexServers = FakeData.GetPlexServer().Generate(10);
        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(plexAccount, plexServers);

        var list = plexServers
            .Select(x => new PlexServerAccessDTO
            {
                PlexServer = x,
                AccessToken = serverAccessTokens.FirstOrDefault(y => y.MachineIdentifier == x.MachineIdentifier)!,
            })
            .ToList();

        mock.Mock<IPlexApiService>()
            .Setup(x => x.GetAccessiblePlexServersAsync(It.IsAny<int>()))
            .ReturnsAsync(Result.Ok(list));

        mock.SetupMediator(It.IsAny<AddOrUpdatePlexServersCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<AddOrUpdatePlexAccountServersCommand>).ReturnsAsync(Result.Ok());
        mock.SendRefreshNotification();

        // Act

        var request = new RefreshPlexServerAccessCommand(plexAccount.Id);
        var handler = mock.Create<RefreshPlexServerAccessCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }
}
