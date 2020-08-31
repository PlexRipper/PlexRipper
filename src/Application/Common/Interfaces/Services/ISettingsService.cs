using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.Common
{
    public interface ISettingsService
    {
        Task<Result<PlexAccount>> SetActivePlexAccountAsync(int accountId);
        Task<Result<PlexAccount>> GetActivePlexAccountAsync();
    }
}
