using AutoMapper;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI;

public class ResultDTOToResult : ITypeConverter<ResultDTO, Result>
{
    public Result Convert(ResultDTO source, Result destination, ResolutionContext context)
    {
        if (source.IsSuccess)
        {
            var result = Result.Ok();
            result.WithReasons(source.Successes.Select(x => new Success(x.Message).WithMetadata(x.Metadata)));
            return result;
        }
        else
        {
            var result = new Result();
            result.WithReasons(source.Errors.Select(x => new Error(x.Message).WithMetadata(x.Metadata)));
            return result;
        }
    }
}