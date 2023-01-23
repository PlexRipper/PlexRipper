using System.Diagnostics;
using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class RemoveInaccessibleServersCommandValidator : AbstractValidator<RemoveInaccessibleServersCommand> { }

public class RemoveInaccessibleServersCommandHandler : BaseHandler, IRequestHandler<RemoveInaccessibleServersCommand, Result>
{
    public RemoveInaccessibleServersCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result> Handle(RemoveInaccessibleServersCommand command, CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var servers = await PlexServerQueryable
            .IncludeLibrariesWithMedia()
            .IncludeLibrariesWithDownloadTasks()
            .Include(x => x.ServerStatus)
            .Include(x => x.PlexAccountServers)
            .Include(x => x.PlexLibraries)
            .ThenInclude(x => x.PlexAccountLibraries)
            .AsTracking()
            .ToListAsync(cancellationToken);

        var accountServers = await _dbContext.PlexAccountServers.Include(x => x.PlexServer).ToListAsync(cancellationToken);

        var serverRemovalList = servers.Where(x => !accountServers.Select(y => y.PlexServerId).Contains(x.Id)).ToList();

        if (serverRemovalList.Count > 0)
        {
            _dbContext.PlexServers.RemoveRange(serverRemovalList);
            await SaveChangesAsync();
        }

        stopwatch.Stop();
        Log.Debug($"{nameof(RemoveInaccessibleServersCommandHandler)} was completed in {stopwatch.Elapsed.TotalSeconds}!");
        return Result.Ok();
    }
}