using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.FolderPaths
{
    public class CreateFolderPathCommand: IRequest<Result<int>>
    {
        public FolderPath FolderPath { get; }

        public CreateFolderPathCommand(FolderPath folderPath)
        {
            FolderPath = folderPath;
        }
    }

}