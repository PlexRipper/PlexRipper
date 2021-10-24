using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class GetAllDownloadTasksQueryValidator : AbstractValidator<GetAllDownloadTasksQuery> { }

    public class GetAllDownloadTasksQueryHandler : BaseHandler, IRequestHandler<GetAllDownloadTasksQuery, Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<DownloadTask>>> Handle(GetAllDownloadTasksQuery request, CancellationToken cancellationToken)
        {
            // AsTracking due to Children->Parent cycle error, therefore all navigation properties are added as well
            var downloadList = await DownloadTasksQueryable
                .AsTracking()
                .IncludeDownloadTasks(true, true)
                .Where(x => x.ParentId == null) // Where clause is to retrieve only the root DownloadTasks
                .ToListAsync(cancellationToken);
            return Result.Ok(downloadList);
        }

        private static List<DownloadTask> BuildDownloadTaskTree(List<DownloadTask> downloadTasks, List<int> downloadTaskIds)
        {
            var flatten = downloadTasks.Flatten(x => x.Children).ToList();

            var rootTasks = new List<DownloadTask>();
            var childTasks = flatten.FindAll(x => x.Parent is not null);

            var tvShows = flatten.FindAll(x => x.DownloadTaskType is DownloadTaskType.TvShow).ToList();
            foreach (var tvShow in tvShows)
            {
                tvShow.Children = childTasks.FindAll(x => x.DownloadTaskType is DownloadTaskType.Season && x.ParentId == tvShow.Id);
                childTasks.RemoveAll(x => tvShow.Children.Select(y => y.Id).Contains(x.Id));

                foreach (var season in tvShow.Children)
                {
                    season.Children = childTasks.FindAll(x => x.DownloadTaskType is DownloadTaskType.Episode && x.ParentId == season.Id);
                    childTasks.RemoveAll(x => season.Children.Select(y => y.Id).Contains(x.Id));

                    foreach (var episode in season.Children)
                    {
                        episode.Children = childTasks.FindAll(x => x.IsDataOrPart() && x.ParentId == episode.Id);
                        childTasks.RemoveAll(x => episode.Children.Select(y => y.Id).Contains(x.Id));
                    }
                }
            }

            rootTasks.AddRange(tvShows);

            rootTasks = MergeInto(rootTasks, childTasks);

            // rootTasks = GroupBy(rootTasks);
            return rootTasks;
        }

        private static DownloadTask InvertDownloadTask(DownloadTask downloadTask)
        {
            if (downloadTask.Parent is not null)
            {
                downloadTask.Parent.Children = new List<DownloadTask> { downloadTask };
                return InvertDownloadTask(downloadTask.Parent);
            }

            return downloadTask;
        }

        private static List<DownloadTask> MergeInto(List<DownloadTask> baseDownloadTasks, List<DownloadTask> newList)
        {
            for (var i = 0; i < newList.Count; i++)
            {
                var newDownloadTask = newList[i];

                if (newDownloadTask.DownloadTaskType is DownloadTaskType.Episode)
                {
                    var tvShow = newDownloadTask.Parent.Parent;
                    tvShow.Children = new List<DownloadTask>();
                    var season = newDownloadTask.Parent;
                    season.Children = new List<DownloadTask> { newDownloadTask };
                    tvShow.Children.Add(season);

                    newDownloadTask = tvShow;
                }

                if (newDownloadTask.DownloadTaskType is DownloadTaskType.TvShow)
                {
                    var tvShow = baseDownloadTasks.Find(x => x.Id == newDownloadTask.Id);
                    if (tvShow is null)
                    {
                        baseDownloadTasks.Add(newDownloadTask);
                        tvShow = baseDownloadTasks.Last();
                    }

                    foreach (var season in newDownloadTask.Children)
                    {
                        var baseSeason = tvShow.Children.Find(x => x.Id == season.Id);
                        if (baseSeason is null)
                        {
                            tvShow.Children.Add(season);
                            baseSeason = tvShow.Children.Last();
                        }

                        foreach (var episode in season.Children)
                        {
                            var baseEpisode = baseSeason.Children.Find(x => x.Id == episode.Id);
                            if (baseEpisode is null)
                            {
                                baseSeason.Children.Add(episode);
                            }
                        }
                    }
                }
            }

            return baseDownloadTasks;
        }
    }
}