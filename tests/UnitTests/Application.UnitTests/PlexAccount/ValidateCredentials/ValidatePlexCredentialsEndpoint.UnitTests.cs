namespace Reaparr.Application.UnitTests;

public class ValidatePlexCredentialsEndpointUnitTests : BaseEndpointUnitTest<ValidatePlexCredentialsEndpoint,
    ValidatePlexCredentialsEndpointRequest, ResultDTO<ValidatePlexCredentialsDTO>>
{
    [Test]
    public async Task ShouldReturnValidatedAccount_WhenSignInSucceeds()
    {
        // Arrange
        var seed = new Seed(2058);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();

        var testAccountResponse = testAccountDTO.ToModel();
        testAccountResponse.UpdateInitProperty(nameof(testAccountResponse.ValidatedAt), DateTime.UtcNow);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Ok(
                    new PlexSignInCommandResult
                    {
                        ClientId = testAccountResponse.ClientId,
                        Username = testAccountResponse.Username,
                        Password = testAccountResponse.Password,
                        Email = testAccountResponse.Email,
                        Title = testAccountResponse.Title,
                        PlexId = testAccountResponse.PlexId,
                        Uuid = testAccountResponse.Uuid,
                        AuthenticationToken = testAccountResponse.AuthenticationToken,
                        IsValidated = testAccountResponse.IsValidated,
                        ValidatedAt = testAccountResponse.ValidatedAt,
                        Is2Fa = testAccountResponse.Is2Fa,
                    }
                )
            );

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new ValidatePlexCredentialsEndpointRequest
            {
                ClientId = testAccountDTO.ClientId,
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            }
        );
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        value.IsValidated.ShouldBeTrue();
        value.ValidatedAt.ShouldNotBeNull();
        value.ValidatedAt?.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        value.Is2Fa.ShouldBeFalse();
        value.Email.ShouldBe(testAccountDTO.Email);
        value.Username.ShouldBe(testAccountDTO.Username);
        value.Password.ShouldBe(testAccountDTO.Password);
        value.AuthenticationToken.ShouldNotBeEmpty();
        value.ClientId.ShouldBe(testAccountDTO.ClientId);
        value.Title.ShouldBe(testAccountDTO.Title);
        value.PlexId.ShouldBe(testAccountDTO.PlexId);
        value.Uuid.ShouldBe(testAccountDTO.Uuid);

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<PlexSignInCommand>(c =>
                            c.Username == testAccountDTO.Username
                            && c.Password == testAccountDTO.Password
                            && c.VerificationCode == testAccountDTO.VerificationCode
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldUseProvidedClientId_WhenClientIdIsPresent()
    {
        // Arrange
        var seed = new Seed(4088);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();

        var signInValue = new PlexSignInCommandResult
        {
            ClientId = testAccountDTO.ClientId,
            Username = testAccountDTO.Username,
            Password = testAccountDTO.Password,
            Email = testAccountDTO.Email,
            Title = testAccountDTO.Title,
            PlexId = testAccountDTO.PlexId,
            Uuid = testAccountDTO.Uuid,
            AuthenticationToken = testAccountDTO.AuthenticationToken,
            IsValidated = true,
            ValidatedAt = DateTime.UtcNow,
            Is2Fa = false,
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(signInValue))
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new ValidatePlexCredentialsEndpointRequest
            {
                ClientId = testAccountDTO.ClientId,
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            }
        );
        var result = endpointResult.Response;

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        value.IsValidated.ShouldBeTrue();
        value.ValidatedAt.ShouldNotBeNull();
        value.ValidatedAt?.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        value.Is2Fa.ShouldBeFalse();
        value.ClientId.ShouldBe(testAccountDTO.ClientId);
        value.Username.ShouldBe(testAccountDTO.Username);
        value.Password.ShouldBe(testAccountDTO.Password);
        value.Email.ShouldBe(testAccountDTO.Email);
        value.Title.ShouldBe(testAccountDTO.Title);
        value.PlexId.ShouldBe(testAccountDTO.PlexId);
        value.Uuid.ShouldBe(testAccountDTO.Uuid);
        value.AuthenticationToken.ShouldBe(testAccountDTO.AuthenticationToken);

        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<PlexSignInCommand>(c =>
                            c.ClientId == testAccountDTO.ClientId
                            && c.Username == testAccountDTO.Username
                            && c.Password == testAccountDTO.Password
                            && c.VerificationCode == testAccountDTO.VerificationCode
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldGenerateClientId_WhenClientIdIsMissing()
    {
        // Arrange
        var seed = new Seed(5091);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();

        var signInValue = new PlexSignInCommandResult
        {
            ClientId = Guid.NewGuid().ToString(),
            Username = testAccountDTO.Username,
            Password = testAccountDTO.Password,
            Email = testAccountDTO.Email,
            Title = testAccountDTO.Title,
            PlexId = testAccountDTO.PlexId,
            Uuid = testAccountDTO.Uuid,
            AuthenticationToken = testAccountDTO.AuthenticationToken,
            IsValidated = true,
            ValidatedAt = DateTime.UtcNow,
            Is2Fa = false,
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(signInValue))
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new ValidatePlexCredentialsEndpointRequest
            {
                ClientId = "",
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            }
        );
        var result = endpointResult.Response;

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeFalse();
        value.IsValidated.ShouldBeTrue();
        value.ValidatedAt.ShouldNotBeNull();
        value.Is2Fa.ShouldBeFalse();
        value.Username.ShouldBe(testAccountDTO.Username);
        value.Password.ShouldBe(testAccountDTO.Password);
        value.Email.ShouldBe(testAccountDTO.Email);
        value.Title.ShouldBe(testAccountDTO.Title);
        value.PlexId.ShouldBe(testAccountDTO.PlexId);
        value.Uuid.ShouldBe(testAccountDTO.Uuid);
        value.AuthenticationToken.ShouldBe(testAccountDTO.AuthenticationToken);

        // The endpoint must have generated a GUID clientId and it must appear in the response.
        value.ClientId.ShouldNotBeNullOrEmpty();
        Guid.Parse(value.ClientId).ShouldNotBe(Guid.Empty);

        Mock.Mock<ICommandExecutor>().Verify();

        // The command must have been sent with a generated clientId, not an empty one.
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<PlexSignInCommand>(c => !string.IsNullOrWhiteSpace(c.ClientId) && c.ClientId.Length > 10),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldRequireVerificationCode_WhenThePlexAPIRespondsWithA2Fa()
    {
        // Arrange
        var seed = new Seed(203958);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.IsValidated = false;
        testAccountDTO.ValidatedAt = null;
        testAccountDTO.CustomAuthenticationToken = string.Empty;

        var signInValue = new PlexSignInCommandResult
        {
            ClientId = testAccountDTO.ClientId,
            Username = testAccountDTO.Username,
            Password = testAccountDTO.Password,
            Email = testAccountDTO.Email,
            Title = testAccountDTO.Title,
            PlexId = testAccountDTO.PlexId,
            Uuid = testAccountDTO.Uuid,
            AuthenticationToken = testAccountDTO.AuthenticationToken,
            IsValidated = false,
            ValidatedAt = null,
            Is2Fa = true,
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result
                    .Ok(signInValue)
                    .WithError(new PlexError("Enter verification code") { Code = PlexErrorCodes.EnterVerificationCode })
            );

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new ValidatePlexCredentialsEndpointRequest
            {
                ClientId = testAccountDTO.ClientId,
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            }
        );
        var result = endpointResult.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        var value = result.Value;
        value.ShouldNotBeNull();
        value.IsUnAuthorized.ShouldBeTrue();
        value.IsValidated.ShouldBeFalse();
        value.ValidatedAt.ShouldBe(null);
        value.Is2Fa.ShouldBeTrue();
        value.Username.ShouldBe(testAccountDTO.Username);
        value.Password.ShouldBe(testAccountDTO.Password);
        value.ClientId.ShouldBe(testAccountDTO.ClientId);
        value.Email.ShouldBeEmpty();
        value.Title.ShouldBeEmpty();
        value.PlexId.ShouldBe(0);
        value.Uuid.ShouldBeEmpty();
        value.AuthenticationToken.ShouldBeEmpty();

        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Test]
    public async Task ShouldNotMarkUnauthorized_WhenUnhandledPlexErrorsOccur()
    {
        // Arrange
        var seed = new Seed(203958);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();
        testAccountDTO.IsValidated = false;
        testAccountDTO.ValidatedAt = null;
        testAccountDTO.CustomAuthenticationToken = string.Empty;

        var signInValue = new PlexSignInCommandResult
        {
            ClientId = testAccountDTO.ClientId,
            Username = testAccountDTO.Username,
            Password = testAccountDTO.Password,
            Email = testAccountDTO.Email,
            Title = testAccountDTO.Title,
            PlexId = testAccountDTO.PlexId,
            Uuid = testAccountDTO.Uuid,
            AuthenticationToken = testAccountDTO.AuthenticationToken,
            IsValidated = false,
            ValidatedAt = null,
            Is2Fa = false,
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result
                    .Ok(signInValue)
                    .WithErrors([new PlexError("Error #1") { Code = 1234 }, new PlexError("Error #2") { Code = 5678 }])
            )
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new ValidatePlexCredentialsEndpointRequest
            {
                ClientId = testAccountDTO.ClientId,
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            }
        );
        var result = endpointResult.Response;

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Count.ShouldBe(2);
        result.Errors.ShouldContain(e => e.Message == "Error #1");
        result.Errors.ShouldContain(e => e.Message == "Error #2");
        result.Value.ShouldBeNull();

        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenCommandExecutionFails()
    {
        // Arrange
        var seed = new Seed(203964);
        var testAccountDTO = FakeData.GetPlexAccount(seed).Generate().ToDTO();

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<PlexSignInCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Command execution failed"))
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new ValidatePlexCredentialsEndpointRequest
            {
                ClientId = testAccountDTO.ClientId,
                DisplayName = testAccountDTO.DisplayName,
                Username = testAccountDTO.Username,
                Password = testAccountDTO.Password,
                VerificationCode = testAccountDTO.VerificationCode,
            }
        );
        var result = endpointResult.Response;

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldHaveSingleItem();
        result.Errors[0].Message.ShouldBe("Command execution failed");
        result.Value.ShouldBeNull();

        Mock.Mock<ICommandExecutor>().Verify();
    }
}