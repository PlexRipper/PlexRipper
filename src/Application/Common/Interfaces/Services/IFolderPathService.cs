using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IFolderPathService
    {
        Task<Result<List<FolderPath>>> GetAllFolderPathsAsync();
    }
}