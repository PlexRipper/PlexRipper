namespace PlexRipper.Application
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