using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain;

namespace PlexRipper.FileSystem
{
    public class FileManagement : IFileManagement
    {
        public void CombineFiles(List<string> filePaths, string outputFilePath, string fileName)
        {
            if (!filePaths.Any())
            {
                return;
            }

            Log.Debug($"Combining {filePaths.Count} into a single file");
            var task = Task.Factory.StartNew(() =>
            {
                using (var outputStream = File.Create(outputFilePath, 2048, FileOptions.Asynchronous))
                {
                    foreach (var filePath in filePaths)
                    {
                        using (var inputStream = File.OpenRead(filePath))
                        {
                            // Buffer size can be passed as the second argument.
                            inputStream.CopyTo(outputStream, 2048);
                        }
                        Log.Debug($"The file at {filePath} has been combined into {fileName}");
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}