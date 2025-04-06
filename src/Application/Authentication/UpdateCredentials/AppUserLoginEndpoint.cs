using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Application;

public record UpdateCredentialsEndpointRequest
{
    /// <summary>
    ///  The username of the <see cref="AppUser"/>.
    /// <para> The default username is <see cref="DefaultUserAppCredentials.DefaultPassword"/>. </para>
    /// </summary>
    public required string? Username { get; init; }

    /// <summary>
    /// The password of the <see cref="AppUser"/>.
    /// <para> The default password is <see cref="DefaultUserAppCredentials.DefaultPassword"/>. </para>
    /// </summary>
    public required string? Password { get; init; }
}

public class UpdateCredentialsEndpointRequestValidator : Validator<UpdateCredentialsEndpointRequest>
{
    public UpdateCredentialsEndpointRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !(x.Username == null && x.Password == null))
            .WithMessage("Both Username and Password cannot be null.");
        RuleFor(x => x.Username).NotEmpty().MinimumLength(8).When(x => x.Username != null);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).When(x => x.Password != null);
    }
}

public class UpdateCredentialsEndpoint : BaseEndpoint<UpdateCredentialsEndpointRequest>
{
    public override string EndpointPath => ApiRoutes.AuthenticatedController;

    private readonly ILog _log;
    private readonly UserManager<AppUser> _userManager;

    public UpdateCredentialsEndpoint(ILog log, UserManager<AppUser> userManager)
    {
        _log = log;
        _userManager = userManager;
    }

    public override void Configure()
    {
        Put(EndpointPath);

        Summary(s =>
        {
            s.Summary = "Updates the credentials of a user.";
            s.ExampleRequest = new UpdateCredentialsEndpointRequest
            {
                Username = DefaultUserAppCredentials.DefaultUsername,
                Password = DefaultUserAppCredentials.DefaultPassword,
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

    public override async Task HandleAsync(UpdateCredentialsEndpointRequest req, CancellationToken ct)
    {
        // There is only 1 app user in the database
        var user = await _userManager.Users.FirstOrDefaultAsync(ct);
        if (user is null)
        {
            var result = Result.Fail("No app user found in the database").LogError();
            await SendFluentResult(result, ct);
            return;
        }

        var newUsername = req.Username;
        var newPassword = req.Password;

        if (newUsername is not null)
        {
            // Update the username
            user.UserName = newUsername;
            user.NormalizedUserName = newUsername.ToUpper();

            var usernameResult = await _userManager.UpdateAsync(user);
            if (!usernameResult.Succeeded)
            {
                var result = Result.Fail(usernameResult.Errors.Select(e => e.Description)).LogError();
                result.Add400BadRequestError("Failed to update username");
                await SendFluentResult(result, ct);
                return;
            }
        }

        if (newPassword is not null)
        {
            // Update the password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!passwordResult.Succeeded)
            {
                var result = Result.Fail(passwordResult.Errors.Select(e => e.Description)).LogError();
                result.Add400BadRequestError("Failed to update password");
                await SendFluentResult(result, ct);
                return;
            }
        }

        _log.WarningLine("The PlexRipper app credentials have been updated! Make sure this is intended");

        // Respond with success
        await SendFluentResult(Result.Ok(), ct);
    }
}
