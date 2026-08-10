namespace Reaparr.Application.UnitTests;

public class RefreshPlexAccountAccessCommandUnitTests : BaseUnitTest<RefreshPlexAccountAccessCommandHandler>
{
    [Test]
    public async Task ShouldValidateAndRefreshServerAndLibraryAccess_WhenTokenIsValid()
    {
        await SetupDatabase(77601, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);
        var serverRapport = new RefreshPlexServerAccessRapport(plexAccount.Id, plexAccount.DisplayName);
        serverRapport.Access.Add(new RefreshPlexServerAccessRapportRow(PlexAccessState.Updated, 1, "Server"));

        Mock.SetupCommand(It.IsAny<ValidatePlexTokenCommand>)
            .ReturnsAsync(
                Result.Ok(
                    new ValidatePlexTokenCommandResult
                    {
                        ClientId = plexAccount.ClientId,
                        Username = plexAccount.Username,
                        Email = plexAccount.Email,
                        Title = plexAccount.Title,
                        PlexId = plexAccount.PlexId,
                        Uuid = plexAccount.Uuid,
                        AuthenticationToken = plexAccount.GetAuthToken,
                        IsValidated = true,
                        ValidatedAt = DateTime.UtcNow,
                        Is2Fa = plexAccount.Is2Fa,
                    }
                )
            );
        Mock.SetupCommand(It.IsAny<RefreshPlexServerAccessCommand>).ReturnsAsync(Result.Ok(serverRapport));
        Mock.SetupCommand(It.IsAny<RefreshLibraryAccessCommand>)
            .ReturnsAsync(Result.Ok(new PlexLibraryAccessRefreshResponse { Reports = [], OfflineServers = [] }));
        Mock.Mock<INotificationHubService>()
            .Setup(x => x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>()))
            .Returns(Task.CompletedTask);

        var result = await Sut.ExecuteAsync(new RefreshPlexAccountAccessCommand(), CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<ValidatePlexTokenCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<RefreshPlexServerAccessCommand>(), It.IsAny<CancellationToken>()),
                Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<RefreshLibraryAccessCommand>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Test]
    public async Task ShouldMarkAccountInvalidAndKeepEnabled_WhenTokenIsUnauthorized()
    {
        await SetupDatabase(77602, config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.AsTracking().FirstAsync(CancellationToken);
        plexAccount.IsValidated = true;
        plexAccount.ValidatedAt = DateTime.UtcNow;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.SetupCommand(It.IsAny<ValidatePlexTokenCommand>)
            .ReturnsAsync(Result.Fail<ValidatePlexTokenCommandResult>("Unauthorized").AddPlex401UnauthorizedError());
        Mock.Mock<INotificationHubService>()
            .Setup(x => x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>()))
            .Returns(Task.CompletedTask);

        var result = await Sut.ExecuteAsync(new RefreshPlexAccountAccessCommand(), CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var updatedAccount = await IDbContext.PlexAccounts.FirstAsync(CancellationToken);
        updatedAccount.IsValidated.ShouldBeFalse();
        updatedAccount.ValidatedAt.ShouldBeNull();
        updatedAccount.IsEnabled.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<RefreshPlexServerAccessCommand>(), It.IsAny<CancellationToken>()),
                Times.Never());
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<RefreshLibraryAccessCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}