using System.ComponentModel;
using Application.Contracts;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Application;

public record AppUserLoginEndpointRequest()
{
    /// <summary>
    ///  The username of the <see cref="AppUser"/>.
    /// </summary>
    [DefaultValue(DefaultUserAppCredentials.DefaultUsername)]
    public required string Username { get; init; }

    /// <summary>
    ///  The password of the <see cref="AppUser"/>.
    ///  <para> The default password is <see cref="DefaultUserAppCredentials.DefaultPassword"/>. </para>
    /// </summary>
    [DefaultValue(DefaultUserAppCredentials.DefaultPassword)]
    public required string Password { get; init; }
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
    public override string EndpointPath => ApiRoutes.LoginController;

    private readonly SignInManager<AppUser> _signInManager;

    public AppUserLoginEndpoint(SignInManager<AppUser> signInManager)
    {
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
            };
        });

        Description(x =>
        {
            x.AutoTagOverride("Authentication");
            x.Produces(StatusCodes.Status200OK);
            x.Produces(StatusCodes.Status401Unauthorized);
            x.Produces(StatusCodes.Status500InternalServerError);
        });
    }

    public override async Task HandleAsync(AppUserLoginEndpointRequest req, CancellationToken ct)
    {
        // Attempt to sign in the user
        var result = await _signInManager.PasswordSignInAsync(
            req.Username,
            req.Password,
            isPersistent: false,
            lockoutOnFailure: false
        );

        if (result.Succeeded)
        {
            await CookieAuth.SignInAsync(u => u.Roles.Add(DefaultUserAppCredentials.DefaultAdminRole));

            await SendOkAsync(ct);
        }
        else if (result.IsLockedOut)
        {
            await SendForbiddenAsync(ct);
        }
        else
        {
            await SendUnauthorizedAsync(ct);
        }
    }
}
