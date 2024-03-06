using Data.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

public record GetDownloadTaskByIdQuery(
    Guid DownloadTaskId,
    DownloadTaskType Type = DownloadTaskType.None
) : IRequest<Result<DownloadTaskGeneric>>;

public class GetDownloadTaskByIdQueryValidator : AbstractValidator<GetDownloadTaskByIdQuery>
{
    public GetDownloadTaskByIdQueryValidator()
    {
        RuleFor(x => x.DownloadTaskId).NotEmpty();
    }
}

public class GetDownloadTaskByIdQueryHandler : IRequestHandler<GetDownloadTaskByIdQuery, Result<DownloadTaskGeneric>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetDownloadTaskByIdQueryHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DownloadTaskGeneric>> Handle(GetDownloadTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(request.DownloadTaskId, request.Type, cancellationToken);

        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTask), request.DownloadTaskId);

        return Result.Ok(downloadTask);
    }
}