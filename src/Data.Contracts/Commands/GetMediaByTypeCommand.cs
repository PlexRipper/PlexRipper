using FastEndpoints;

namespace Reaparr.Data.Contracts;

public record GetMediaByTypeCommand : ICommand<Result<PagedMediaQueryResult>>
{
    public required MediaQueryFilter Filter { get; init; }
}
