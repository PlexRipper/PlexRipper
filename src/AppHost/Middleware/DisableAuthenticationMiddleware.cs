using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Reaparr.AppHost;

/// <summary>
/// Development-only middleware that bypasses all authentication by injecting a synthetic Admin principal.
/// Activated by setting <c>I_AM_DUMB_SO_DISABLE_AUTHENTICATION=true</c> in the environment.
/// </summary>
public sealed class DisableAuthenticationMiddleware(RequestDelegate next)
{
    private static readonly ClaimsPrincipal _adminPrincipal = CreateAdminPrincipal();

    /// <summary>
    /// Injects a synthetic Admin principal for every request, bypassing all authentication.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        context.User = _adminPrincipal;
        await context.SignInAsync(_adminPrincipal);

        await next(context);
    }

    private static ClaimsPrincipal CreateAdminPrincipal()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "dev-user"),
            new Claim(ClaimTypes.NameIdentifier, "dev-user-id"),
            new Claim(ClaimTypes.Email, "dev@reaparr.dev"),
            new Claim(ClaimTypes.Role, "Admin"),
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "DisabledAuth");
        return new ClaimsPrincipal(identity);
    }
}
