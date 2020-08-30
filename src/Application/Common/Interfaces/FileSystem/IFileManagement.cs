using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.Common.Interfaces.FileSystem
{
    public interface IFileManagement
    {
        void AddFileTask(FileTask fileTask);
    }
}