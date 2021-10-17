using System.Collections.Generic;
using System.Linq;
using Bogus;
using Environment;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public static partial class FakeData
    {
        private static int _plexMovieDownloadTaskId = 1;

        #region Movie

        public static Faker<DownloadTask> GetMovieDownloadTask(FakeDataConfig config = null)
        {
            config ??= new FakeDataConfig();

            return new Faker<DownloadTask>()
                .StrictMode(true)
                .RuleFor(x => x.Id, _ => _plexMovieDownloadTaskId++)
                .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Initialized)
                .RuleFor(x => x.Priority, _ => 0)
                .RuleFor(x => x.DataReceived, _ => 0)
                .RuleFor(x => x.DataTotal, f => f.Random.Long(1, 10000000))
                .RuleFor(x => x.DownloadWorkerTasks, _ => new())
                .RuleFor(x => x.MediaType, PlexMediaType.Movie)
                .RuleFor(x => x.Key, _ => _random.Next(0, 10000))
                .RuleFor(x => x.Created, f => f.Date.Recent(30))
                .RuleFor(x => x.PlexServerId, f => f.Random.Int(1, 1000))
                .RuleFor(x => x.PlexServer, _ => new PlexServer())
                .RuleFor(x => x.PlexLibraryId, f => f.Random.Int(1, 10000))
                .RuleFor(x => x.PlexLibrary, _ => new PlexLibrary())
                .RuleFor(x => x.DownloadFolder, () => new FolderPath
                {
                    DirectoryPath = PathSystem.RootDirectory,
                })
                .RuleFor(x => x.DownloadFolderId, _ => 1)
                .RuleFor(x => x.DestinationFolder, () => new FolderPath
                {
                    DirectoryPath = PathSystem.RootDirectory,
                })
                .RuleFor(x => x.DestinationFolderId, _ => 2)
                .FinishWith((_, u) =>
                {
                    u.DownloadWorkerTasks = new List<DownloadWorkerTask>
                    {
                        new(u, 1, 0, u.DataTotal),
                    };
                });
        }

        #endregion
    }
}