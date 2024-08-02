using FluentResults;

namespace Application.Contracts;

public static class ResultMapper
{
    public static Result<T> ToResultModel<T>(this ResultDTO<T> resultDTO)
    {
        var result = new Result<T>();
        result.WithValue(resultDTO.Value);
        result.WithReasons(resultDTO.Reasons);
        result.WithErrors(resultDTO.Errors);
        result.WithSuccesses(resultDTO.Successes);
        return result;
    }
}
