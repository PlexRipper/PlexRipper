using Data.Contracts;
using FluentValidation;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class AddDownloadWorkerLogsValidator : AbstractValidator<AddDownloadWorkerLogsCommand> { }

public class AddDownloadWorkerLogsHandler : BaseHandler, IRequestHandler<AddDownloadWorkerLogsCommand, Result>
{
    public AddDownloadWorkerLogsHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result> Handle(AddDownloadWorkerLogsCommand command, CancellationToken cancellationToken)
    {
        await _dbContext.DownloadWorkerTasksLogs.AddRangeAsync(command.DownloadWorkerLogs, cancellationToken);
        await SaveChangesAsync();
        return Result.Ok();
    }
}