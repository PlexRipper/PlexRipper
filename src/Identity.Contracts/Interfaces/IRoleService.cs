using Microsoft.AspNetCore.Identity;

namespace Reaparr.Identity.Contracts;

/// <summary>
/// Service interface for role management operations
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Checks if a role exists
    /// </summary>
    Task<bool> RoleExistsAsync(string roleName);

    /// <summary>
    /// Creates a new role
    /// </summary>
    Task<IdentityResult> CreateAsync(IdentityRole role);
}
