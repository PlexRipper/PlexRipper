using Moq;
using PlexRipper.Application.PlexAccounts;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.UnitTests
{
    public class PlexAccountService_CreatePlexAccountAsync_UnitTests
    {
        private readonly Mock<PlexAccountService> _sut;

        private readonly Mock<IMediator> _iMediator = new();

        private readonly Mock<IPlexServerService> _plexServerService = new();

        private readonly Mock<IPlexApiService> _plexApiService = new();

        private readonly Mock<ISchedulerService> _schedulerService = new();

        public PlexAccountService_CreatePlexAccountAsync_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            _sut = new Mock<PlexAccountService>(MockBehavior.Strict, _iMediator.Object, _plexServerService.Object, _plexApiService.Object,
                _schedulerService.Object);
        }

        [Fact]
        public async Task CreatePlexAccountAsync_ShouldSuccessResult_WhenAccountIsValid()
        {
            // Arrange
            var newAccount = new PlexAccount("TestUsername", "Password123");

            _sut.Setup(x => x.CheckIfUsernameIsAvailableAsync(newAccount.Username)).ReturnsAsync(Result.Ok(true));
            _sut.Setup(x => x.ValidatePlexAccountAsync(newAccount)).ReturnsAsync(Result.Ok());
            _sut.Setup(x => x.SetupAccountAsync(It.IsAny<int>())).ReturnsAsync(Result.Ok());
            _iMediator.Setup(m => m.Send(It.IsAny<CreatePlexAccountCommand>(), CancellationToken.None)).ReturnsAsync(Result.Ok(1));
            _iMediator.Setup(m => m.Send(It.IsAny<GetPlexAccountByIdQuery>(), CancellationToken.None)).ReturnsAsync(Result.Ok(new PlexAccount()));

            // Act
            var result = await _sut.Object.CreatePlexAccountAsync(newAccount);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountAlreadyExists()
        {
            // Arrange
            var newAccount = new PlexAccount("TestUsername", "Password123");
            _sut.Setup(x => x.CheckIfUsernameIsAvailableAsync(newAccount.Username)).ReturnsAsync(Result.Ok(false));

            // Act
            var result = await _sut.Object.CreatePlexAccountAsync(newAccount);

            // Assert
            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountUsernameExistenceCheckFailed()
        {
            // Arrange
            var newAccount = new PlexAccount("TestUsername", "Password123");
            _sut.Setup(x => x.CheckIfUsernameIsAvailableAsync(newAccount.Username)).ReturnsAsync(Result.Fail("Error #1"));

            // Act
            var result = await _sut.Object.CreatePlexAccountAsync(newAccount);

            // Assert
            result.IsFailed.ShouldBeTrue();
            result.Errors.First().Message.ShouldBe("Error #1");
        }

        [Fact]
        public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountCreationFailed()
        {
            // Arrange
            var newAccount = new PlexAccount("TestUsername", "Password123");
            _sut.Setup(x => x.CheckIfUsernameIsAvailableAsync(newAccount.Username)).ReturnsAsync(Result.Ok(true));
            _sut.Setup(x => x.ValidatePlexAccountAsync(newAccount)).ReturnsAsync(Result.Ok());
            _iMediator.Setup(m => m.Send(It.IsAny<CreatePlexAccountCommand>(), CancellationToken.None)).ReturnsAsync(Result.Fail("Error #1"));

            // Act
            var result = await _sut.Object.CreatePlexAccountAsync(newAccount);

            // Assert
            result.IsFailed.ShouldBeTrue();
            result.Errors.First().Message.ShouldBe("Error #1");
        }

        [Fact]
        public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountSetupFailed()
        {
            // Arrange
            var newAccount = new PlexAccount("TestUsername", "Password123");
            _sut.Setup(x => x.CheckIfUsernameIsAvailableAsync(newAccount.Username)).ReturnsAsync(Result.Ok(true));
            _sut.Setup(x => x.ValidatePlexAccountAsync(newAccount)).ReturnsAsync(Result.Ok());
            _iMediator.Setup(m => m.Send(It.IsAny<CreatePlexAccountCommand>(), CancellationToken.None)).ReturnsAsync(Result.Ok(1));
            _sut.Setup(x => x.SetupAccountAsync(It.IsAny<int>())).ReturnsAsync(Result.Fail("Error #1"));

            // Act
            var result = await _sut.Object.CreatePlexAccountAsync(newAccount);

            // Assert
            result.IsFailed.ShouldBeTrue();
            result.Errors.First().Message.ShouldBe("Error #1");
        }
    }
}