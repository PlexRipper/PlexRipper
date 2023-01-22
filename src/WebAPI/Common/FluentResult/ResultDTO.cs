using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.FluentResult;

public class ResultDTO
{
    [JsonProperty("isFailed", Required = Required.Always)]
    public bool IsFailed { get; set; }

    [JsonProperty("isSuccess", Required = Required.Always)]
    public bool IsSuccess { get; set; }

    [JsonProperty("reasons", Required = Required.Always)]
    public List<ReasonDTO> Reasons { get; set; } = new();

    [JsonProperty("errors", Required = Required.Always)]
    public List<ErrorDTO> Errors { get; set; } = new();

    [JsonProperty("successes", Required = Required.Always)]
    public List<SuccessDTO> Successes { get; set; } = new();
}

public class ResultDTO<T> : ResultDTO
{
    [JsonProperty("value", Required = Required.Always)]
    public T Value { get; set; }
}

public class ReasonDTO : IReason
{
    [JsonProperty("message", Required = Required.Always)]
    public string Message { get; set; }

    [JsonProperty("metadata", Required = Required.Always)]
    public Dictionary<string, object> Metadata { get; set; }
}

public class ErrorDTO
{
    public List<ErrorDTO> Reasons { get; set; }

    [JsonProperty("message", Required = Required.Always)]
    public string Message { get; set; }

    [JsonProperty("metadata", Required = Required.Always)]
    public Dictionary<string, object> Metadata { get; set; }
}

public class SuccessDTO : ISuccess
{
    [JsonProperty("message", Required = Required.Always)]
    public string Message { get; set; }

    [JsonProperty("metadata", Required = Required.Always)]
    public Dictionary<string, object> Metadata { get; set; }
}