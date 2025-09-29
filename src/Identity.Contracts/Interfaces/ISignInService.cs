using Microsoft.AspNetCore.Identity;

namespace Reaparr.Identity.Contracts;

/// <summary>
/// Service interface for sign-in operations
/// </summary>
public interface ISignInService
{
    /// <summary>
    /// Attempts to sign in a user with username and password
    /// </summary>
    Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);

    /// <summary>
    /// Signs out the current user
    /// </summary>
    Task SignOutAsync();
}
