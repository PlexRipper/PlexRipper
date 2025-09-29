using System.Security.Claims;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using Reaparr.Identity.Contracts;

namespace Reaparr.Identity.Services;

/// <summary>
/// Identity-based implementation of ISignInService
/// </summary>
public class IdentityIdentitySignInService : IIdentitySignInService
{
    private readonly SignInManager<AppUser> _signInManager;

    public IdentityIdentitySignInService(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    /// <inheritdoc/>
    public async Task<SignInResult> PasswordSignInAsync(
        string userName,
        string password,
        bool isPersistent,
        bool lockoutOnFailure
    ) => await _signInManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
    
    /// <inheritdoc/>
    public async Task SignInAsync(IEnumerable<Claim> claims, IEnumerable<string> roles)
    {
        await CookieAuth.SignInAsync(u =>
        {
            u.Claims.AddRange(claims);
            u.Roles.AddRange(roles);
        });
    }

    /// <inheritdoc/>
    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
