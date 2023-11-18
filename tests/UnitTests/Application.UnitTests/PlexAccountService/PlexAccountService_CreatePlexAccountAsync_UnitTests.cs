using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using PlexApi.Contracts;
using PlexRipper.Application.PlexAccounts;

namespace PlexRipper.Application.UnitTests;

public class PlexAccountService_CreatePlexAccountAsync_UnitTests : BaseUnitTest<PlexAccountService>
{
    public PlexAccountService_CreatePlexAccountAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldSuccessResult_WhenAccountIsValid()
    {
        // Arrange
        var newAccount = new PlexAccount("TestUsername", "Password123");

        mock.Mock<IPlexAccountService>().Setup(x => x.ValidatePlexAccountAsync(newAccount)).ReturnsAsync(Result.Ok());
        mock.Mock<IInspectServerScheduler>().Setup(x => x.QueueInspectPlexServerByPlexAccountIdJob(1)).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<CheckUsernameTaskRequest>).ReturnsAsync(Result.Ok(true));
        mock.SetupMediator(It.IsAny<CreatePlexAccountCommand>).ReturnsAsync(Result.Ok(1));
        mock.SetupMediator(It.IsAny<GetPlexAccountByUsernameQuery>).ReturnsAsync(Result.Fail("").Add404NotFoundError());
        mock.SetupMediator(It.IsAny<GetPlexAccountByIdQuery>).ReturnsAsync(Result.Ok(new PlexAccount()));

        // Act
        var result = await _sut.CreatePlexAccountAsync(newAccount);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountAlreadyExists()
    {
        // Arrange
        var newAccount = new PlexAccount("TestUsername", "Password123");
        mock.SetupMediator(It.IsAny<CheckUsernameTaskRequest>).ReturnsAsync(Result.Ok(false));
        mock.SetupMediator(It.IsAny<GetPlexAccountByUsernameQuery>).ReturnsAsync(Result.Ok(new PlexAccount()));

        // Act
        var result = await _sut.CreatePlexAccountAsync(newAccount);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountUsernameExistenceCheckFailed()
    {
        // Arrange
        var newAccount = new PlexAccount("TestUsername", "Password123");
        mock.SetupMediator(It.IsAny<CheckUsernameTaskRequest>).ReturnsAsync(Result.Fail("Error #1"));
        mock.SetupMediator(It.IsAny<GetPlexAccountByUsernameQuery>).ReturnsAsync(Result.Fail("Error #1"));

        // Act
        var result = await _sut.CreatePlexAccountAsync(newAccount);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldBe("Error #1");
    }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountCreationFailed()
    {
        // Arrange
        var newAccount = new PlexAccount("TestUsername", "Password123");
        mock.SetupMediator(It.IsAny<CheckUsernameTaskRequest>).ReturnsAsync(Result.Ok(true));
        mock.Mock<IPlexAccountService>().Setup(x => x.ValidatePlexAccountAsync(newAccount)).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<CreatePlexAccountCommand>).ReturnsAsync(Result.Fail("Error #1"));
        mock.SetupMediator(It.IsAny<GetPlexAccountByUsernameQuery>).ReturnsAsync(Result.Fail("Error #1").Add404NotFoundError());

        // Act
        var result = await _sut.CreatePlexAccountAsync(newAccount);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldBe("Error #1");
    }
}