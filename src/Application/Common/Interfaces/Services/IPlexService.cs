using PlexRipper.Domain.ValueObjects;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexService
    {
        Task<PlexAccount> RequestTokenAsync(string username, string password);
    }
}
