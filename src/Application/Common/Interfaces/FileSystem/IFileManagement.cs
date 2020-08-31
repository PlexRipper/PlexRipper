using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.Common
{
    public interface IFileManagement
    {
        void AddFileTask(FileTask fileTask);
    }
}