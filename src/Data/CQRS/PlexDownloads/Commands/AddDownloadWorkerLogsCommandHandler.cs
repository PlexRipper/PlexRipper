using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class AddDownloadWorkerLogsValidator : AbstractValidator<AddDownloadWorkerLogsCommand> { }

public class AddDownloadWorkerLogsHandler : BaseHandler, IRequestHandler<AddDownloadWorkerLogsCommand, Result>
{
    public AddDownloadWorkerLogsHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(AddDownloadWorkerLogsCommand command, CancellationToken cancellationToken)
    {
        await _dbContext.DownloadWorkerTasksLogs.AddRangeAsync(command.DownloadWorkerLogs, cancellationToken);
        await SaveChangesAsync();
        return Result.Ok();
    }
}