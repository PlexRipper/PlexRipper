﻿using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class GetDownloadTaskByIdQueryValidator : AbstractValidator<GetDownloadTaskByIdQuery>
    {
        public GetDownloadTaskByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetDownloadTaskByIdQueryHandler : BaseHandler, IRequestHandler<GetDownloadTaskByIdQuery, Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<DownloadTask>> Handle(GetDownloadTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var query = DownloadTasksQueryable;

            if (request.IncludeChildren)
            {
                query = query.AsTracking()
                        .Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .Include(x => x.DestinationFolder)
                        .Include(x => x.DownloadFolder)
                        .Include(x => x.DownloadWorkerTasks)
                        .Include(x => x.Children)

                        // Level 1
                        .Include(x => x.Children).ThenInclude(x => x.PlexServer)
                        .Include(x => x.Children).ThenInclude(x => x.PlexLibrary)
                        .Include(x => x.Children).ThenInclude(x => x.DestinationFolder)
                        .Include(x => x.Children).ThenInclude(x => x.DownloadFolder)
                        .Include(x => x.Children).ThenInclude(x => x.DownloadWorkerTasks)

                        // Level 2
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.PlexServer)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.PlexLibrary)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.DestinationFolder)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.DownloadFolder)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.DownloadWorkerTasks)

                        // Level 3
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.PlexServer)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.PlexLibrary)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.DestinationFolder)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.DownloadFolder)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.DownloadWorkerTasks)

                        // Level 4
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.PlexServer)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.PlexLibrary)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.DestinationFolder)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.DownloadFolder)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.DownloadWorkerTasks)

                        // Level 5
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.Children).ThenInclude(x => x.PlexServer)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.Children).ThenInclude(x => x.PlexLibrary)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.Children).ThenInclude(x => x.DestinationFolder)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.Children).ThenInclude(x => x.DownloadFolder)
                        .Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                        .ThenInclude(x => x.Children).ThenInclude(x => x.DownloadWorkerTasks);


            }

            var downloadTask = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            return ReturnResult(downloadTask, request.Id);
        }
    }
}