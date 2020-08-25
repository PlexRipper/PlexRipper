using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.FileSystem
{
    public interface IFileManagement
    {
        void CombineFiles(List<string> filePaths, string outputFilePath, string fileName);
    }
}