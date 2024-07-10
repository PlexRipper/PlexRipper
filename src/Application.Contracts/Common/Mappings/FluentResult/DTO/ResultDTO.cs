using FluentResults;

namespace Application.Contracts;

public class ResultDTO
{
    public required bool IsFailed { get; set; }

    public required bool IsSuccess { get; set; }

    public required List<ReasonDTO> Reasons { get; set; } = new();

    public required List<ErrorDTO> Errors { get; set; } = new();

    public required List<SuccessDTO> Successes { get; set; } = new();
}

public class ResultDTO<T> : ResultDTO
{
    public T Value { get; set; }
}

public class ReasonDTO : IReason
{
    public required string Message { get; set; }

    public required Dictionary<string, object> Metadata { get; set; }
}

public class ErrorDTO : IError
{
    public required List<IError> Reasons { get; set; }

    public required string Message { get; set; }

    public required Dictionary<string, object> Metadata { get; set; }
}

public class SuccessDTO : ISuccess
{
    public required string Message { get; set; }

    public required Dictionary<string, object> Metadata { get; set; }
}
