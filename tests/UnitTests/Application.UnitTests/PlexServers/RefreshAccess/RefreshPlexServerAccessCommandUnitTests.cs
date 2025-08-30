using Application.Contracts;
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
        await SetupDatabase(
            66197,
            config =>
            {
                config.PlexAccountCount = 1;
            }
        );

        var plexAccount = await IDbContext.PlexAccounts.FirstOrDefaultAsync(CancellationToken);
        plexAccount.ShouldNotBeNull();

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAccessiblePlexServersCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlexServerAccessDTO>());

        // Act
        var request = new RefreshPlexServerAccessCommand(plexAccount.Id);
        var handler = mock.Create<RefreshPlexServerAccessCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldReturnOkResult_WhenThereAreAccessiblePlexServers()
    {
        // Arrange
        var seed = await SetupDatabase(
            65148,
            config =>
            {
                config.PlexAccountCount = 1;
            }
        );

        var plexAccount = await IDbContext.PlexAccounts.FirstOrDefaultAsync(CancellationToken);
        plexAccount.ShouldNotBeNull();

        var plexServers = FakeData.GetPlexServer(seed).Generate(10);
        var serverAccessTokens = FakeData.GetServerAccessTokenDTO(seed, plexAccount, plexServers);

        var list = plexServers
            .Select(x => new PlexServerAccessDTO
            {
                PlexServer = x,
                AccessToken = serverAccessTokens.FirstOrDefault(y => y.MachineIdentifier == x.MachineIdentifier)!,
            })
            .ToList();

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAccessiblePlexServersCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(list));

        mock.SetupCommand(It.IsAny<AddOrUpdatePlexServersCommand>).ReturnsAsync(Result.Ok());
        mock.SetupCommand(It.IsAny<AddOrUpdatePlexAccountServersCommand>)
            .ReturnsAsync(Result.Ok(new RefreshPlexServerAccessRapport(plexAccount.Id, plexAccount.DisplayName)));
        mock.SetupCommand(It.IsAny<RefreshLibraryAccessCommand>)
            .ReturnsAsync(Result.Ok(new PlexLibraryAccessRefreshResponse { OfflineServers = [], Reports = [] }));

        mock.SendRefreshNotification();

        // Act

        var request = new RefreshPlexServerAccessCommand(plexAccount.Id);
        var handler = mock.Create<RefreshPlexServerAccessCommandHandler>();
        var result = await handler.ExecuteAsync(request, CancellationToken);

        // Assert
        result.ShouldNotBeNull();
    }
}
