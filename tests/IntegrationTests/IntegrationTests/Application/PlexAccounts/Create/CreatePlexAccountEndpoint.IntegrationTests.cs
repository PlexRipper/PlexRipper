using System.Net;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.IntegrationTests;

public class CreatePlexAccountEndpointIntegrationTests : BaseIntegrationTests
{
    public CreatePlexAccountEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCreatePlexAccountAndInspectServers_WhenValidDataProvided()
    {
        // Arrange
        var seed = new Seed(236234);
        var serverCount = 1;
        var libraryCount = 3;

        using var container = await CreateContainer(
            seed.Next(),
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    // Start from an empty database
                    x.PlexServerCount = 0;
                    x.PlexMovieLibraryCount = 0;
                    x.PlexAccountCount = 0;
                    x.PlexServerConnectionPerServerCount = 1;
                };

                config.BaseMockHttpClientOptions = x =>
                {
                    x.PlexServerAccessCount = serverCount;
                    x.MovieLibraryCount = libraryCount;
                    x.MoviesPerLibraryCount = 500;
                };
            }
        );

        var plexAccount = FakeData.GetPlexAccount(4347564).Generate();
        var request = new CreatePlexAccountEndpointRequest
        {
            DisplayName = plexAccount.DisplayName,
            Username = plexAccount.Username,
            Password = plexAccount.Password,
            IsEnabled = plexAccount.IsEnabled,
            IsMain = plexAccount.IsMain,
            IsValidated = plexAccount.IsValidated,
            ValidatedAt = plexAccount.ValidatedAt,
            Uuid = plexAccount.Uuid,
            PlexId = plexAccount.PlexId,
            Email = plexAccount.Email,
            Title = plexAccount.Title,
            ClientId = plexAccount.ClientId,
            Is2Fa = plexAccount.Is2Fa,
            CustomAuthenticationToken = plexAccount.CustomAuthenticationToken,
            AuthenticationToken = plexAccount.AuthenticationToken,
        };

        // Act
        var client = container.GetApiClient();
        await client.SignIn();
        var response = await client.POSTAsync<
            CreatePlexAccountEndpoint,
            CreatePlexAccountEndpointRequest,
            ResultDTO<PlexAccountDTO>
        >(request);

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Result.IsSuccess.ShouldBeTrue();
        response.Result.Errors.ShouldBeEmpty();

        await container.SchedulerService.AwaitScheduler(CancellationToken);

        // Add a small delay to ensure database transactions complete after job execution
        await Task.Delay(1000, CancellationToken);

        // Wait for database to be in the expected state with increased timeout for complex job chains
        await WaitForDatabaseConditionAsync(
            () =>
                container.DbContext.PlexAccounts.Include(x => x.PlexAccountLibraries).First().PlexAccountLibraries.Count
                == libraryCount,
            maxRetries: 20, // Increased from default 10 to 20 (10 seconds total)
            delayMs: 500
        );

        // Assert database state
        container.DbContext.PlexAccounts.ToList().Count.ShouldBe(1);

        var plexAccountDb = container
            .DbContext.PlexAccounts.Include(x => x.PlexAccountLibraries)
            .ThenInclude(x => x.PlexLibrary)
            .Include(x => x.PlexAccountServers)
            .ThenInclude(x => x.PlexServer)
            .FirstOrDefault();

        plexAccountDb.ShouldNotBeNull();
        plexAccountDb.IsValidated.ShouldBeTrue();
        plexAccountDb.PlexServers.Count.ShouldBe(serverCount);
        plexAccountDb.DisplayName.ShouldBe(request.DisplayName);
        plexAccountDb.Username.ShouldBe(request.Username);
        plexAccountDb.Password.ShouldBe(request.Password);
        plexAccountDb.PlexAccountLibraries.Count.ShouldBe(libraryCount);

        // Ensure PlexServer has been created
        container.DbContext.PlexServers.ToList().Count.ShouldBe(serverCount);
        var plexServersDb = container
            .DbContext.PlexServers.Include(x => x.PlexLibraries)
            .IncludeLibrariesWithMedia()
            .FirstOrDefault();
        plexServersDb.ShouldNotBeNull();
        plexServersDb.MachineIdentifier.ShouldNotBeEmpty();
        plexServersDb.PlexLibraries.Count.ShouldBe(libraryCount);

        // Ensure all jobs have sent notifications
        var jobStatusUpdateList = container.MockSignalRService.JobStatusUpdateList.ToList();
        jobStatusUpdateList.Count.ShouldBe(4);

        jobStatusUpdateList[0].JobType.ShouldBe(JobTypes.InspectPlexServerJob);
        jobStatusUpdateList[0].Status.ShouldBe(JobStatus.Started);
        jobStatusUpdateList[1].JobType.ShouldBe(JobTypes.InspectPlexServerJob);
        jobStatusUpdateList[1].Status.ShouldBe(JobStatus.Completed);
        jobStatusUpdateList[2].JobType.ShouldBe(JobTypes.SyncServerMediaJob);
        jobStatusUpdateList[2].Status.ShouldBe(JobStatus.Started);
        jobStatusUpdateList[3].JobType.ShouldBe(JobTypes.SyncServerMediaJob);
        jobStatusUpdateList[3].Status.ShouldBe(JobStatus.Completed);
    }

    [Fact]
    public async Task ShouldReturn400_WhenUsernameAlreadyExists()
    {
        // Arrange
        var seed = new Seed(78901);
        using var container = await CreateContainer(
            seed.Next(),
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexServerCount = 1;
                    x.PlexAccountCount = 1; // Pre-create one account
                };
            }
        );

        // Get the existing account from the database
        var existingAccount = await container.DbContext.PlexAccounts.FirstAsync(CancellationToken);

        // Create request with the same username
        var plexAccount = FakeData.GetPlexAccount(seed.Next()).Generate();
        var request = new CreatePlexAccountEndpointRequest
        {
            DisplayName = plexAccount.DisplayName,
            Username = existingAccount.Username, // Use existing username
            Password = plexAccount.Password,
            IsEnabled = plexAccount.IsEnabled,
            IsMain = plexAccount.IsMain,
            IsValidated = plexAccount.IsValidated,
            ValidatedAt = plexAccount.ValidatedAt,
            Uuid = plexAccount.Uuid,
            PlexId = plexAccount.PlexId,
            Email = plexAccount.Email,
            Title = plexAccount.Title,
            ClientId = plexAccount.ClientId,
            Is2Fa = plexAccount.Is2Fa,
            CustomAuthenticationToken = plexAccount.CustomAuthenticationToken,
            AuthenticationToken = plexAccount.AuthenticationToken,
        };

        // Act
        var client = container.GetApiClient();
        await client.SignIn();
        var response = await client.POSTAsync<
            CreatePlexAccountEndpoint,
            CreatePlexAccountEndpointRequest,
            ResultDTO<PlexAccountDTO>
        >(request);

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Result.IsSuccess.ShouldBeFalse();
        response.Result.Errors.ShouldNotBeEmpty();
        response.Result.Errors[0].Message.ShouldContain("username");
        response.Result.Errors[0].Message.ShouldContain("already existing");

        // Ensure no additional account was created
        container.DbContext.PlexAccounts.Count().ShouldBe(1);
    }

    [Fact]
    public async Task ShouldReturn400_WhenUuidAlreadyExists()
    {
        // Arrange
        var seed = new Seed(45678);
        using var container = await CreateContainer(
            seed.Next(),
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexServerCount = 1;
                    x.PlexAccountCount = 1; // Pre-create one account
                };
            }
        );

        // Get the existing account from the database
        var existingAccount = await container.DbContext.PlexAccounts.FirstAsync(CancellationToken);

        // Create request with the same UUID
        var plexAccount = FakeData.GetPlexAccount(seed.Next()).Generate();
        var request = new CreatePlexAccountEndpointRequest
        {
            DisplayName = plexAccount.DisplayName,
            Username = plexAccount.Username,
            Password = plexAccount.Password,
            IsEnabled = plexAccount.IsEnabled,
            IsMain = plexAccount.IsMain,
            IsValidated = plexAccount.IsValidated,
            ValidatedAt = plexAccount.ValidatedAt,
            Uuid = existingAccount.Uuid, // Use existing UUID
            PlexId = plexAccount.PlexId,
            Email = plexAccount.Email,
            Title = plexAccount.Title,
            ClientId = plexAccount.ClientId,
            Is2Fa = plexAccount.Is2Fa,
            CustomAuthenticationToken = plexAccount.CustomAuthenticationToken,
            AuthenticationToken = plexAccount.AuthenticationToken,
        };

        // Act
        var client = container.GetApiClient();
        await client.SignIn();
        var response = await client.POSTAsync<
            CreatePlexAccountEndpoint,
            CreatePlexAccountEndpointRequest,
            ResultDTO<PlexAccountDTO>
        >(request);

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Result.IsSuccess.ShouldBeFalse();
        response.Result.Errors.ShouldNotBeEmpty();
        response.Result.Errors[0].Message.ShouldContain("UUID");
        response.Result.Errors[0].Message.ShouldContain("already exists");

        // Ensure no additional account was created
        container.DbContext.PlexAccounts.Count().ShouldBe(1);
    }

    [Fact]
    public async Task ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var seed = new Seed(12345);
        using var container = await CreateContainer(seed.Next());

        var request = new CreatePlexAccountEndpointRequest
        {
            DisplayName = "", // Invalid - empty
            Username = "",
            Password = "",
            IsEnabled = false,
            IsMain = false,
            IsValidated = false, // Invalid - should be true
            ValidatedAt = null, // Invalid - should not be null
            Uuid = "",
            PlexId = 0,
            Email = "",
            Title = "",
            ClientId = "",
            Is2Fa = false,
            CustomAuthenticationToken = "",
            AuthenticationToken = "",
        };

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        // Use PostAsync directly to get raw HTTP response
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json"
        );
        var httpResponse = await client.PostAsync(ApiRoutes.PlexAccountController + "/", content, CancellationToken);

        // Assert
        httpResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Read the response body to verify it contains validation errors
        var responseBody = await httpResponse.Content.ReadAsStringAsync(CancellationToken);
        responseBody.ShouldNotBeNullOrEmpty();
        responseBody.ShouldContain("errors"); // FastEndpoints validation response contains errors

        // Ensure no account was created
        container.DbContext.PlexAccounts.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ShouldUseProvidedClientId_WhenProvided()
    {
        // Arrange
        var seed = new Seed(55555);
        var expectedClientId = Guid.NewGuid().ToString();
        using var container = await CreateContainer(
            seed.Next(),
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexServerCount = 0;
                    x.PlexAccountCount = 0;
                };

                config.BaseMockHttpClientOptions = x =>
                {
                    x.PlexServerAccessCount = 0; // No servers to inspect
                };
            }
        );

        var plexAccount = FakeData.GetPlexAccount(seed.Next()).Generate();
        var request = new CreatePlexAccountEndpointRequest
        {
            DisplayName = plexAccount.DisplayName,
            Username = plexAccount.Username,
            Password = plexAccount.Password,
            IsEnabled = plexAccount.IsEnabled,
            IsMain = plexAccount.IsMain,
            IsValidated = plexAccount.IsValidated,
            ValidatedAt = plexAccount.ValidatedAt,
            Uuid = plexAccount.Uuid,
            PlexId = plexAccount.PlexId,
            Email = plexAccount.Email,
            Title = plexAccount.Title,
            ClientId = expectedClientId, // Provide specific ClientId
            Is2Fa = plexAccount.Is2Fa,
            CustomAuthenticationToken = plexAccount.CustomAuthenticationToken,
            AuthenticationToken = plexAccount.AuthenticationToken,
        };

        // Act
        var client = container.GetApiClient();
        await client.SignIn();
        var response = await client.POSTAsync<
            CreatePlexAccountEndpoint,
            CreatePlexAccountEndpointRequest,
            ResultDTO<PlexAccountDTO>
        >(request);

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Result.IsSuccess.ShouldBeTrue();
        response.Result.Value.ShouldNotBeNull();
        response.Result.Value.ClientId.ShouldBe(expectedClientId);

        // Verify in database
        var plexAccountDb = await container.DbContext.PlexAccounts.FirstAsync(CancellationToken);
        plexAccountDb.ClientId.ShouldBe(expectedClientId);
    }

    [Fact]
    public async Task ShouldCreatePlexAccount_WhenUsingAuthTokenMode()
    {
        // Arrange
        var seed = new Seed(99999);
        using var container = await CreateContainer(
            seed.Next(),
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexServerCount = 0;
                    x.PlexAccountCount = 0;
                };

                config.BaseMockHttpClientOptions = x =>
                {
                    x.PlexServerAccessCount = 0;
                };
            }
        );

        var plexAccount = FakeData.GetPlexAccount(seed.Next()).Generate();
        var request = new CreatePlexAccountEndpointRequest
        {
            DisplayName = plexAccount.DisplayName,
            Username = "", // Empty in auth token mode
            Password = "", // Empty in auth token mode
            IsEnabled = plexAccount.IsEnabled,
            IsMain = plexAccount.IsMain,
            IsValidated = plexAccount.IsValidated,
            ValidatedAt = plexAccount.ValidatedAt,
            Uuid = plexAccount.Uuid,
            PlexId = plexAccount.PlexId,
            Email = plexAccount.Email,
            Title = plexAccount.Title,
            ClientId = plexAccount.ClientId,
            Is2Fa = plexAccount.Is2Fa,
            CustomAuthenticationToken = "custom-token-12345", // Using custom auth token
            AuthenticationToken = plexAccount.AuthenticationToken,
        };

        // Act
        var client = container.GetApiClient();
        await client.SignIn();
        var response = await client.POSTAsync<
            CreatePlexAccountEndpoint,
            CreatePlexAccountEndpointRequest,
            ResultDTO<PlexAccountDTO>
        >(request);

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Result.IsSuccess.ShouldBeTrue();
        response.Result.Value.ShouldNotBeNull();
        response.Result.Value.CustomAuthenticationToken.ShouldBe("custom-token-12345");

        // Verify in database
        var plexAccountDb = await container.DbContext.PlexAccounts.FirstAsync(CancellationToken);
        plexAccountDb.CustomAuthenticationToken.ShouldBe("custom-token-12345");
        plexAccountDb.Username.ShouldBe(""); // Should remain empty in auth token mode
        plexAccountDb.Password.ShouldBe(""); // Should remain empty in auth token mode
    }

    [Fact]
    public async Task ShouldReturnCreatedAccount_WhenSuccessful()
    {
        // Arrange
        var seed = new Seed(33333);
        using var container = await CreateContainer(
            seed.Next(),
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexServerCount = 0;
                    x.PlexAccountCount = 0;
                };

                config.BaseMockHttpClientOptions = x =>
                {
                    x.PlexServerAccessCount = 0;
                };
            }
        );

        var plexAccount = FakeData.GetPlexAccount(seed.Next()).Generate();
        var request = new CreatePlexAccountEndpointRequest
        {
            DisplayName = plexAccount.DisplayName,
            Username = plexAccount.Username,
            Password = plexAccount.Password,
            IsEnabled = plexAccount.IsEnabled,
            IsMain = plexAccount.IsMain,
            IsValidated = plexAccount.IsValidated,
            ValidatedAt = plexAccount.ValidatedAt,
            Uuid = plexAccount.Uuid,
            PlexId = plexAccount.PlexId,
            Email = plexAccount.Email,
            Title = plexAccount.Title,
            ClientId = plexAccount.ClientId,
            Is2Fa = plexAccount.Is2Fa,
            CustomAuthenticationToken = plexAccount.CustomAuthenticationToken,
            AuthenticationToken = plexAccount.AuthenticationToken,
        };

        // Act
        var client = container.GetApiClient();
        await client.SignIn();
        var response = await client.POSTAsync<
            CreatePlexAccountEndpoint,
            CreatePlexAccountEndpointRequest,
            ResultDTO<PlexAccountDTO>
        >(request);

        // Assert
        response.Response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Result.IsSuccess.ShouldBeTrue();
        response.Result.Value.ShouldNotBeNull();
        response.Result.Value.DisplayName.ShouldBe(request.DisplayName);
        response.Result.Value.Username.ShouldBe(request.Username);
        response.Result.Value.Email.ShouldBe(request.Email);
        response.Result.Value.Uuid.ShouldBe(request.Uuid);
        response.Result.Value.PlexId.ShouldBe(request.PlexId);
        response.Result.Value.IsEnabled.ShouldBe(request.IsEnabled);
        response.Result.Value.IsValidated.ShouldBe(request.IsValidated);
        response.Result.Value.IsMain.ShouldBe(request.IsMain);
        response.Result.Value.Is2Fa.ShouldBe(request.Is2Fa);
        response.Result.Value.Id.ShouldBeGreaterThan(0);
    }
}
