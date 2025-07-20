using FluentResults;

namespace Application.Contracts;

/// <summary>
/// The <see cref="ResultDTO{T}"/> without the value.
/// NOTE: This is named BaseResultDTO to allow for type generating and using this as a base. In TypeScript this works differently than C#
/// </summary>
public class BaseResultDTO
{
    public required bool IsSuccess { get; init; } = true;

    public required int StatusCode { get; set; }

    public required List<ErrorDTO> Errors { get; set; } = [];

    public required List<SuccessDTO> Successes { get; set; } = [];
}

public class ResultDTO<T> : BaseResultDTO
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
