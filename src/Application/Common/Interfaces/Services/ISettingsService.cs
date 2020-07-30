using FluentResults;
using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface ISettingsService
    {
        Task<Result<PlexAccount>> SetActivePlexAccountAsync(int accountId);
        Task<Result<PlexAccount>> GetActivePlexAccountAsync();
    }
}
