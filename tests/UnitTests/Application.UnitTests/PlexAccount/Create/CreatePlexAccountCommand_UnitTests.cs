using Application.Contracts;
using BackgroundServices.Contracts;

namespace PlexRipper.Application.UnitTests;

public class CreatePlexAccountCommand_UnitTests : BaseUnitTest<CreatePlexAccountHandler>
{
    public CreatePlexAccountCommand_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldSuccessResult_WhenAccountIsValid()
    {
        // Arrange
        await SetupDatabase();
        var newAccount = new PlexAccount("TestUsername", "Password123");

        mock.SetupMediator(It.IsAny<IsUsernameAvailableQuery>).ReturnsAsync(Result.Ok(true));
        mock.Mock<IInspectServerScheduler>().Setup(x => x.QueueInspectPlexServerByPlexAccountIdJob(1)).ReturnsAsync(Result.Ok());

        // Act
        var handler = new CreatePlexAccountHandler(_log, GetDbContext(), mock.Create<IMediator>(), mock.Create<IInspectServerScheduler>());
        var result = await handler.Handle(new CreatePlexAccountCommand(newAccount), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountAlreadyExists()
    {
        // Arrange
        await SetupDatabase();
        var newAccount = new PlexAccount("TestUsername", "Password123");

        mock.SetupMediator(It.IsAny<IsUsernameAvailableQuery>).ReturnsAsync(Result.Ok(false));
        mock.Mock<IInspectServerScheduler>().Setup(x => x.QueueInspectPlexServerByPlexAccountIdJob(1)).ReturnsAsync(Result.Ok());

        // Act
        var handler = new CreatePlexAccountHandler(_log, GetDbContext(), mock.Create<IMediator>(), mock.Create<IInspectServerScheduler>());
        var result = await handler.Handle(new CreatePlexAccountCommand(newAccount), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountUsernameExistenceCheckFailed()
    {
        // Arrange
        await SetupDatabase();
        var newAccount = new PlexAccount("TestUsername", "Password123");

        mock.SetupMediator(It.IsAny<IsUsernameAvailableQuery>).ReturnsAsync(Result.Fail("Error #1"));
        mock.Mock<IInspectServerScheduler>().Setup(x => x.QueueInspectPlexServerByPlexAccountIdJob(1)).ReturnsAsync(Result.Ok());

        // Act
        var handler = new CreatePlexAccountHandler(_log, GetDbContext(), mock.Create<IMediator>(), mock.Create<IInspectServerScheduler>());
        var result = await handler.Handle(new CreatePlexAccountCommand(newAccount), CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldBe("Error #1");
    }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountCreationFailed()
    {
        // Arrange
        await SetupDatabase();
        var newAccount = new PlexAccount("TestUsername", "Password123");

        mock.SetupMediator(It.IsAny<IsUsernameAvailableQuery>).ReturnsAsync(Result.Fail("Error #1"));
        mock.Mock<IInspectServerScheduler>().Setup(x => x.QueueInspectPlexServerByPlexAccountIdJob(1)).ReturnsAsync(Result.Ok());

        // Act
        var handler = new CreatePlexAccountHandler(_log, GetDbContext(), mock.Create<IMediator>(), mock.Create<IInspectServerScheduler>());
        var result = await handler.Handle(new CreatePlexAccountCommand(newAccount), CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldBe("Error #1");
    }
}