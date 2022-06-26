using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.FluentResult
{
    public class ResultDTO
    {
        [JsonProperty("isFailed", Required = Required.Always)]
        public bool IsFailed { get; set; }

        [JsonProperty("isSuccess", Required = Required.Always)]
        public bool IsSuccess { get; set; }

        [JsonProperty("reasons", Required = Required.Always)]
        public IList<IReason> Reasons { get; set; } = new List<IReason>();

        [JsonProperty("errors", Required = Required.Always)]
        public IList<IError> Errors { get; set; } = new List<IError>();

        [JsonProperty("successes", Required = Required.Always)]
        public IList<ISuccess> Successes { get; set; } = new List<ISuccess>();
    }

    public class ResultDTO<T> : ResultDTO
    {
        [JsonProperty("value", Required = Required.Always)]
        public T Value { get; set; }
    }
}