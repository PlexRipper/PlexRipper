using Microsoft.AspNetCore.Http;
using PlexRipper.Application.Common.Interfaces;
using System.Security.Claims;

namespace PlexRipper.WebAPI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public string UserId { get; }
    }
}
