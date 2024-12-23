using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Application;

public record AppUserLoginEndpointRequest()
{
    public required string Username { get; set; }

    public required string Password { get; set; }
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
        Description(x =>
            x.WithTags("Authentication")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
        );
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
            await CookieAuth.SignInAsync(u =>
            {
                u.Roles.Add("Admin");
                u.Permissions.AddRange(new[] { "Create_Item", "Delete_Item" });
                u.Claims.Add(new("Address", "123 Street"));
            });

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
