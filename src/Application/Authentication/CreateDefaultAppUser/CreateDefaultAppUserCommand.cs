using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Identity.Contracts;
using Reaparr.Logging;
using Reaparr.Settings.Contracts;
using Serilog;

namespace Reaparr.Application;

public record CreateDefaultAppUserCommand : ICommand<Result>;

public class CreateDefaultAppUserCommandValidator : AbstractValidator<CreateDefaultAppUserCommand>
{
    public CreateDefaultAppUserCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class CreateDefaultAppUserCommandHandler : ICommandHandler<CreateDefaultAppUserCommand, Result>
{
    private readonly Serilog.ILogger _log;
    private readonly IAuthenticationSettings _authenticationSettings;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public CreateDefaultAppUserCommandHandler(
        ILogger log,
        IAuthenticationSettings authenticationSettings,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _log = log.ForContext<CreateDefaultAppUserCommandHandler>();
        _authenticationSettings = authenticationSettings;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result> ExecuteAsync(CreateDefaultAppUserCommand command, CancellationToken cancellationToken)
    {
        if (_authenticationSettings.ResetCredentials)
        {
            _log.Here()
                .Warning(
                    "Setting: {ResetCredentials} has been enabled! Resetting Reaparr app username and password!",
                    nameof(_authenticationSettings.ResetCredentials)
                );
            var toBeDeletedUser = await _userManager.Users.FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (toBeDeletedUser != null)
            {
                _log.Here().Information("Reaparr app user was found, deleting now and creating the default one.");
                await _userManager.DeleteAsync(toBeDeletedUser);
            }
        }

        // Create default roles if they don't exist
        var adminRole = DefaultUserAppCredentials.DefaultAdminRole;
        var defaultRoles = new[] { adminRole };
        foreach (var role in defaultRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create default user
        var defaultUsername = DefaultUserAppCredentials.DefaultUsername;
        var defaultPassword = DefaultUserAppCredentials.DefaultPassword;
        var defaultUser = new AppUser
        {
            UserName = defaultUsername,
            Email = "admin@default.email.com",
            TwoFactorEnabled = false,
            EmailConfirmed = true,
        };

        var user = await _userManager.Users.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (user is null)
        {
            var result = await _userManager.CreateAsync(defaultUser, defaultPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(defaultUser, adminRole);
                _log.Here()
                    .Warning(
                        "APP USER CREATED: user \"{DefaultUserName}\" with password \"{DefaultPassword}\" created successfully, make sure to update this default user!",
                        defaultUsername,
                        defaultPassword
                    );
            }
            else
            {
                _log.Here().Error("Failed to create default user {DefaultUserName}:", defaultUsername);

                foreach (var error in result.Errors)
                {
                    _log.Here().Error(" - {Error}", error.Description);
                }
            }
        }

        if (_authenticationSettings.ResetCredentials)
        {
            _log.Here()
                .Information(
                    "Setting: {ResetCredentials} back to false!",
                    nameof(_authenticationSettings.ResetCredentials)
                );
            _authenticationSettings.ResetCredentials = false;
        }

        return Result.Ok();
    }
}
