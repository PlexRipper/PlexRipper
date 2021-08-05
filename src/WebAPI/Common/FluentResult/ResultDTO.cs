using System.Collections.Generic;
using FluentResults;
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
        public IList<Reason> Reasons { get; set; } = new List<Reason>();

        [JsonProperty("errors", Required = Required.Always)]
        public IList<Error> Errors { get; set; } = new List<Error>();

        [JsonProperty("successes", Required = Required.Always)]
        public IList<Success> Successes { get; set; } = new List<Success>();
    }

    public class ResultDTO<T> : ResultDTO
    {
        [JsonProperty("value", Required = Required.Always)]
        public T Value { get; set; }
    }
}