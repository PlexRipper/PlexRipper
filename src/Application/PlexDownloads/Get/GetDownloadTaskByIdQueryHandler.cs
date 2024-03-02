using Data.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

public record GetDownloadTaskByIdQuery(
    DownloadTaskType Type,
    Guid DownloadTaskId) : IRequest<Result<DownloadTaskGeneric>>;

public class GetDownloadTaskByIdQueryValidator : AbstractValidator<GetDownloadTaskByIdQuery>
{
    public GetDownloadTaskByIdQueryValidator()
    {
        RuleFor(x => x.DownloadTaskId).NotEmpty();
        RuleFor(x => x.Type).NotEqual(DownloadTaskType.None);
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
        var downloadTask = await _dbContext.GetDownloadTaskByKeyQuery(request.Type, request.DownloadTaskId, cancellationToken);

        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTask), request.DownloadTaskId);

        return Result.Ok(downloadTask);
    }
}