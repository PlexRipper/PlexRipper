using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Logging.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace PlexRipper.Application.UnitTests;

public class CreatePlexAccountEndpoint_UnitTests : BaseUnitTest<CreatePlexAccountEndpoint>
{
    public CreatePlexAccountEndpoint_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldSuccessResult_WhenAccountIsValid()
    {
        // Arrange
        await SetupDatabase();
        var newAccount = PlexAccount.Create("TestUsername", "Password123");

        mock.SetupMediator(It.IsAny<InspectAllPlexServersByAccountIdCommand>).ReturnsAsync(Result.Ok());

        var ep = Factory.Create<CreatePlexAccountEndpoint>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                s.AddTransient(_ => mock.Create<ILog>());
                s.AddTransient(_ => mock.Create<IMediator>());
                s.AddTransient(_ => mock.Create<IPlexRipperDbContext>());
            });
        });

        var request = new CreatePlexAccountEndpointRequest() { PlexAccount = newAccount.ToDTO() };

        // Act
        await ep.HandleAsync(request, default);
        var result = ep.Response;

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountUsernameExistenceCheckFailed()
    {
        // Arrange
        await SetupDatabase(config => config.PlexAccountCount = 1);
        var plexAccount = await IDbContext.PlexAccounts.GetAsync(1);
        plexAccount.ShouldNotBeNull();

        var newAccount = PlexAccount.Create(plexAccount.Username, "Password123");

        mock.SetupMediator(It.IsAny<InspectAllPlexServersByAccountIdCommand>).ReturnsAsync(Result.Ok());

        var ep = Factory.Create<CreatePlexAccountEndpoint>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                s.AddTransient(_ => mock.Create<ILog>());
                s.AddTransient(_ => mock.Create<IMediator>());
                s.AddTransient(_ => mock.Create<IPlexRipperDbContext>());
            });
        });

        var request = new CreatePlexAccountEndpointRequest() { PlexAccount = newAccount.ToDTO() };

        // Act
        await ep.HandleAsync(request, default);
        var result = ep.Response;

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task CreatePlexAccountAsync_ShouldFailedResult_WhenAccountCreationFailed()
    {
        // Arrange
        await SetupDatabase();
        var newAccount = PlexAccount.Create("TestUsername", "Password123");

        mock.SetupMediator(It.IsAny<InspectAllPlexServersByAccountIdCommand>).ReturnsAsync(Result.Fail("Error #1"));

        var ep = Factory.Create<CreatePlexAccountEndpoint>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                s.AddTransient(_ => mock.Create<ILog>());
                s.AddTransient(_ => mock.Create<IMediator>());
                s.AddTransient(_ => mock.Create<IPlexRipperDbContext>());
            });
        });

        var request = new CreatePlexAccountEndpointRequest() { PlexAccount = newAccount.ToDTO() };

        // Act
        await ep.HandleAsync(request, default);
        var result = ep.Response;

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldBe("Error #1");
    }
}
