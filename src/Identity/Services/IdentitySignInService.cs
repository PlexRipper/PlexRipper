using Microsoft.AspNetCore.Identity;
using Reaparr.Identity.Contracts;

namespace Reaparr.Identity.Services;

/// <summary>
/// Identity-based implementation of ISignInService
/// </summary>
public class IdentitySignInService : ISignInService
{
    private readonly SignInManager<AppUser> _signInManager;

    public IdentitySignInService(SignInManager<AppUser> signInManager)
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
    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}