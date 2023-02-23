namespace PlexRipper.WebAPI.Common.FluentResult;

public class ResultDTO
{
    public bool IsFailed { get; set; }

    public bool IsSuccess { get; set; }

    public List<ReasonDTO> Reasons { get; set; } = new();

    public List<ErrorDTO> Errors { get; set; } = new();

    public List<SuccessDTO> Successes { get; set; } = new();
}

public class ResultDTO<T> : ResultDTO
{
    public T Value { get; set; }
}

public class ReasonDTO : IReason
{
    public string Message { get; set; }

    public Dictionary<string, object> Metadata { get; set; }
}

public class ErrorDTO
{
    public List<ErrorDTO> Reasons { get; set; }

    public string Message { get; set; }

    public Dictionary<string, object> Metadata { get; set; }
}

public class SuccessDTO : ISuccess
{
    public string Message { get; set; }

    public Dictionary<string, object> Metadata { get; set; }
}