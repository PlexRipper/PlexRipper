using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Application;

public record CreateDefaultAppUserCommand() : IRequest<Result>;

public class CreateDefaultAppUserCommandValidator : AbstractValidator<CreateDefaultAppUserCommand>
{
    public CreateDefaultAppUserCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class CreateDefaultAppUserCommandHandler : IRequestHandler<CreateDefaultAppUserCommand, Result>
{
    private readonly ILog _log;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public CreateDefaultAppUserCommandHandler(
        ILog log,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _log = log;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result> Handle(CreateDefaultAppUserCommand command, CancellationToken cancellationToken)
    {
        // Create default roles if they don't exist
        var adminRole = DefaultUserAppCredentials.DefaultAdminRole;
        var defaultRoles = new[] { adminRole, "User" };
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

        if (await _userManager.FindByEmailAsync(defaultUser.Email) == null)
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

        return Result.Ok();
    }
}
