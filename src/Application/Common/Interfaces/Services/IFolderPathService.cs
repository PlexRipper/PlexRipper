using FluentResults;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IFolderPathService
    {
        Task<Result<List<FolderPath>>> GetAllFolderPathsAsync();
        Task<Result<FolderPath>> UpdateFolderPathAsync(FolderPath folderPath);
    }
}
