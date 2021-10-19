using System.Collections.Generic;
using Bogus;
using Environment;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public static partial class FakeData
    {
        private static int _plexDownloadTaskId = 1;

        public static Faker<T> ApplyBaseDownloadTask<T>(this Faker<T> faker, FakeDataConfig config = null) where T : DownloadTask
        {
            config ??= new FakeDataConfig();

            return faker
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.Id, _ => _plexDownloadTaskId++)
                .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Initialized)
                .RuleFor(x => x.Title, f => f.Company.CompanyName())
                .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Movie)
                .RuleFor(x => x.Priority, _ => 0)
                .RuleFor(x => x.DataReceived, _ => 0)
                .RuleFor(x => x.DataTotal, f => f.Random.Long(1, 10000000))
                .RuleFor(x => x.DownloadWorkerTasks, _ => new())
                .RuleFor(x => x.FileName, f => f.System.FileName() + ".mp4")
                .RuleFor(x => x.FileLocationUrl, f => f.System.FilePath())
                .RuleFor(x => x.DownloadDirectory, f => f.System.FilePath())
                .RuleFor(x => x.DestinationDirectory, f => f.System.FilePath())
                .RuleFor(x => x.ParentId, _ => null)
                .RuleFor(x => x.Parent, _ => null)
                .RuleFor(x => x.MediaType, PlexMediaType.Movie)
                .RuleFor(x => x.Key, _ => _random.Next(1, 10000))
                .RuleFor(x => x.MediaId, _ => _random.Next(1, 10000))
                .RuleFor(x => x.Created, f => f.Date.Recent(30))
                .RuleFor(x => x.Quality, f => f.PickRandom("sd", "720", "1080"))
                .RuleFor(x => x.PlexServerId, f => f.Random.Int(1, 1000))
                .RuleFor(x => x.PlexServer, _ => new PlexServer())
                .RuleFor(x => x.PlexLibraryId, f => f.Random.Int(1, 10000))
                .RuleFor(x => x.PlexLibrary, _ => new PlexLibrary())
                .RuleFor(x => x.Children, _ => new List<DownloadTask>())
                .RuleFor(x => x.DownloadFolder, () => new FolderPath
                {
                    DirectoryPath = PathSystem.RootDirectory,
                })
                .RuleFor(x => x.DownloadFolderId, _ => 1)
                .RuleFor(x => x.DestinationFolder, () => new FolderPath
                {
                    DirectoryPath = PathSystem.RootDirectory,
                })
                .RuleFor(x => x.DestinationFolderId, _ => 2);
        }

        #region Movie

        public static Faker<DownloadTask> GetMovieDownloadTask(FakeDataConfig config = null)
        {
            config ??= new FakeDataConfig();

            return new Faker<DownloadTask>()
                .ApplyBaseDownloadTask(config)
                .UseSeed(config.Seed)
                .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Movie)
                .RuleFor(x => x.Children, _ => GetMovieDataDownloadTask(config).Generate(1))
                .FinishWith((_, downloadTask) =>
                {
                    downloadTask.Children.ForEach(x =>
                    {
                        x.Parent = x;
                        x.ParentId = x.Id;
                    });
                });
        }

        public static Faker<DownloadTask> GetMovieDataDownloadTask(FakeDataConfig config = null)
        {
            config ??= new FakeDataConfig();

            return new Faker<DownloadTask>()
                .ApplyBaseDownloadTask(config)
                .UseSeed(config.Seed)
                .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.MovieData)
                .FinishWith((_, downloadTask) =>
                {
                    downloadTask.DownloadWorkerTasks = new List<DownloadWorkerTask>
                    {
                        new(downloadTask, 1, 0, downloadTask.DataTotal),
                    };
                });
        }

        #endregion

        #region TvShow

        public static Faker<DownloadTask> GetTvShowDownloadTask(FakeDataConfig config = null)
        {
            config ??= new FakeDataConfig();

            return new Faker<DownloadTask>()
                .ApplyBaseDownloadTask(config)
                .UseSeed(config.Seed)
                .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.TvShow)
                .RuleFor(x => x.Children, _ => GetTvShowSeasonDownloadTask(config).Generate(1))
                .FinishWith((_, downloadTask) =>
                {
                    downloadTask.Children.ForEach(x =>
                    {
                        x.Parent = x;
                        x.ParentId = x.Id;
                    });
                });
        }

        public static Faker<DownloadTask> GetTvShowSeasonDownloadTask(FakeDataConfig config = null)
        {
            config ??= new FakeDataConfig();

            return new Faker<DownloadTask>()
                .ApplyBaseDownloadTask(config)
                .UseSeed(config.Seed)
                .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Season)
                .RuleFor(x => x.Children, _ => GetTvShowEpisodeDownloadTask(config).Generate(config.MediaCount))
                .FinishWith((_, downloadTask) =>
                {
                    downloadTask.Children.ForEach(x =>
                    {
                        x.Parent = x;
                        x.ParentId = x.Id;
                    });
                });
        }

        public static Faker<DownloadTask> GetTvShowEpisodeDownloadTask(FakeDataConfig config = null)
        {
            config ??= new FakeDataConfig();

            return new Faker<DownloadTask>()
                .ApplyBaseDownloadTask(config)
                .UseSeed(config.Seed)
                .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.Episode)
                .RuleFor(x => x.Children, _ => GetTvShowEpisodeDataDownloadTask(config).Generate(config.MediaCount))
                .FinishWith((_, downloadTask) =>
                {
                    downloadTask.Children.ForEach(x =>
                    {
                        x.Parent = x;
                        x.ParentId = x.Id;
                    });
                });
        }

        public static Faker<DownloadTask> GetTvShowEpisodeDataDownloadTask(FakeDataConfig config = null)
        {
            config ??= new FakeDataConfig();

            return new Faker<DownloadTask>()
                .ApplyBaseDownloadTask(config)
                .UseSeed(config.Seed)
                .RuleFor(x => x.DownloadTaskType, _ => DownloadTaskType.EpisodeData);
        }

        #endregion
    }
}