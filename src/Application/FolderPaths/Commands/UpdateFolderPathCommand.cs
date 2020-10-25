using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.FolderPaths.Commands
{
    public class UpdateFolderPathCommand : IRequest<Result<FolderPath>>
    {
        public FolderPath FolderPath { get; }

        public UpdateFolderPathCommand(FolderPath folderPath)
        {
            FolderPath = folderPath;
        }
    }
}