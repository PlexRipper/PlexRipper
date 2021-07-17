using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
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