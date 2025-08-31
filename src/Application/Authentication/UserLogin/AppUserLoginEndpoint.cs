using System.ComponentModel;
using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Reaparr.Application.Contracts;
using Reaparr.Identity.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

public record AppUserLoginEndpointRequest
{
    /// <summary>
    ///  The username of the <see cref="AppUser"/>.
    /// <para> The default username is <see cref="DefaultUserAppCredentials.DefaultPassword"/>. </para>
    /// </summary>
    [DefaultValue(DefaultUserAppCredentials.DefaultUsername)]
    public required string Username { get; init; }

    /// <summary>
    /// The password of the <see cref="AppUser"/>.
    /// <para> The default password is <see cref="DefaultUserAppCredentials.DefaultPassword"/>. </para>
    /// </summary>
    [DefaultValue(DefaultUserAppCredentials.DefaultPassword)]
    public required string Password { get; init; }

    /// <summary>
    /// A value indicating whether the user should be remembered when the browser is closed.
    /// <para> The default value is false </para>
    /// </summary>
    [DefaultValue(false)]
    public required bool RememberMe { get; init; }
};

public class AppUserLoginEndpointRequestValidator : Validator<AppUserLoginEndpointRequest>
{
    public AppUserLoginEndpointRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class AppUserLoginEndpoint : BaseEndpoint<AppUserLoginEndpointRequest>
{
    public override string EndpointPath => ApiRoutes.LoginEndpoint;

    private readonly Serilog.ILogger _log;
    private readonly SignInManager<AppUser> _signInManager;

    public AppUserLoginEndpoint(ILogger log, SignInManager<AppUser> signInManager)
    {
        _log = log.ForContext<AppUserLoginEndpoint>();
        _signInManager = signInManager;
    }

    public override void Configure()
    {
        Post(EndpointPath);
        AllowAnonymous();
        AllowFormData();

        Summary(s =>
        {
            s.Summary = "User Login";
            s.Description = "Logs in a user.";

            s.ExampleRequest = new AppUserLoginEndpointRequest
            {
                Username = DefaultUserAppCredentials.DefaultUsername,
                Password = DefaultUserAppCredentials.DefaultPassword,
                RememberMe = false,
            };
        });

        Description(x =>
        {
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO));
            x.Produces(StatusCodes.Status401Unauthorized, typeof(BaseResultDTO));
            x.Produces(StatusCodes.Status403Forbidden, typeof(BaseResultDTO));
            x.Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO));
        });
    }

    public override async Task HandleAsync(AppUserLoginEndpointRequest req, CancellationToken ct)
    {
        var username = req.Username;
        var password = req.Password;

        _log.Here().Information("Attempting to sign in user {Username}.", username);

        // Attempt to sign in the user
        var signInResult = await _signInManager.PasswordSignInAsync(
            username,
            password,
            isPersistent: req.RememberMe,
            lockoutOnFailure: true
        );

        if (signInResult.Succeeded)
        {
            _log.Here().Information("User {Username} signed in successfully.", username);

            await CookieAuth.SignInAsync(u => u.Roles.Add(DefaultUserAppCredentials.DefaultAdminRole));

            await SendFluentResult(Result.Ok(), ct);
        }
        else if (signInResult.IsLockedOut)
        {
            var result = _log.Here().WarningResult("User {Username} is locked out.", username);
            result.Add403ForbiddenError();

            await SendFluentResult(result, ct);
        }
        else
        {
            var result = _log.Here().WarningResult("Failed to sign in user {Username}.", username);
            result.Add401UnauthorizedError();

            await SendFluentResult(result, ct);
        }
    }
}
