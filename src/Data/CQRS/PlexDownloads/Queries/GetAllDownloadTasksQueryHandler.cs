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

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class GetAllDownloadTasksQueryValidator : AbstractValidator<GetAllDownloadTasksQuery> { }

    public class GetAllDownloadTasksQueryHandler : BaseHandler, IRequestHandler<GetAllDownloadTasksQuery, Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<DownloadTask>>> Handle(GetAllDownloadTasksQuery request, CancellationToken cancellationToken)
        {
            // AsTracking due to Children->Parent cycle error
            var query = DownloadTasksQueryable.AsTracking();

            if (request.IncludeServer)
            {
                query = query.Include(x => x.PlexServer);
            }

            if (request.IncludePlexLibrary)
            {
                query = query.Include(x => x.PlexLibrary);
            }

            if (request.DownloadTaskIds != null && request.DownloadTaskIds.Any())
            {
                var downloadTasks = await query.IncludeDownloadTasks()
                    .Where(x => request.DownloadTaskIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);
                return Result.Ok(BuildDownloadTaskTree(downloadTasks, request.DownloadTaskIds));
            }

            // Where clause is to retrieve only the root DownloadTasks
            var downloadList = await query
                .IncludeDownloadTasks()
                .Where(x => x.ParentId == null)
                .ToListAsync(cancellationToken);
            return Result.Ok(downloadList);
        }

        private static List<DownloadTask> BuildDownloadTaskTree(List<DownloadTask> downloadTasks, List<int> downloadTaskIds)
        {
            var rootTasks = downloadTasks.FindAll(x => x.Parent is null);

            // Find tasks that are not already included in the rootTasks
            var nonRootTasks = downloadTasks.FindAll(x => x.Parent is not null);

            //var invertedTasks = new List<DownloadTask>();

            foreach (var downloadTask in nonRootTasks)
            {
                rootTasks.Add(InvertDownloadTask(downloadTask));
            }

            // foreach (var nonRootTask in nonRootTasks)
            // {
            //     if (nonRootTask.DownloadTaskType is DownloadTaskType.Season)
            //     {
            //         var result = rootTasks.Find(x => x.Id == nonRootTask.ParentId);
            //         if (result is not null)
            //         {
            //             result.Children.Add(nonRootTask);
            //         }
            //         else
            //         {
            //             var newTask = nonRootTask.Parent;
            //             newTask.Children = new List<DownloadTask> { nonRootTask };
            //             rootTasks.Add(newTask);
            //         }
            //     }
            //
            //     if (nonRootTask.DownloadTaskType is DownloadTaskType.Episode)
            //     {
            //         // Is Episode already added
            //         var result = rootTasks.Find(x => x.Children.Any(y => y.Children.Any(z => z.Id == nonRootTask.Id)));
            //         if (result is not null)
            //         {
            //             result = result.Children.Find(x => x.Id == nonRootTask.ParentId);
            //             if (result is not null)
            //             {
            //                 result.Children.Add(nonRootTask);
            //             }
            //         }
            //         else
            //         {
            //             var newTask = InvertDownloadTask(nonRootTask);
            //             rootTasks.Add(newTask);
            //         }
            //     }
            // }

            // var newParentTask = childTasks.Select(x => x.Parent).GroupBy(x => x.Id).Select(x => x.First()).ToList();
            // foreach (var downloadTask in newParentTask)
            // {
            //     downloadTask.Children = childTasks.FindAll(x => x.ParentId == downloadTask.Id);
            // }
            //
            // rootTasks.AddRange(newParentTask);

            rootTasks = GroupBy(rootTasks);
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

        private static List<DownloadTask> GroupBy(List<DownloadTask> downloadTasks)
        {
            foreach (var downloadTask in downloadTasks)
            {
                if (downloadTask.Children.Any())
                {
                    downloadTask.Children = GroupBy(downloadTask.Children);
                    downloadTask.Children = downloadTask.Children.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                }
            }

            return downloadTasks.GroupBy(x => x.Id).Select(x => x.First()).ToList();;
        }
    }
}