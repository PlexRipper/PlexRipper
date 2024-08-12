using FluentResults;

namespace Application.Contracts;

public class ResultDTO
{
    public required bool IsFailed { get; set; } = false;

    public required bool IsSuccess { get; init; } = true;

    public required List<ReasonDTO> Reasons { get; set; } = [];

    public required List<ErrorDTO> Errors { get; set; } = [];

    public required List<SuccessDTO> Successes { get; set; } = [];
}

public class ResultDTO<T> : ResultDTO
{
    public T? Value { get; init; }
}

public class ReasonDTO : IReason
{
    public required string Message { get; init; }

    public required Dictionary<string, object> Metadata { get; init; }
}

public class ErrorDTO : IError
{
    public required List<IError> Reasons { get; init; }

    public required string Message { get; init; }

    public required Dictionary<string, object> Metadata { get; init; }
}

public class SuccessDTO : ISuccess
{
    public required string Message { get; init; }

    public required Dictionary<string, object> Metadata { get; init; }
}
