using System.Collections.Generic;
using FluentResults;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.FluentResult
{
    public class ResultDTO
    {
        [JsonProperty("isFailed")]
        public bool IsFailed { get; set; }

        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty("reasons")]
        public IList<Reason> Reasons { get; set; } = new List<Reason>();

        [JsonProperty("errors")]
        public IList<Error> Errors { get; set; } = new List<Error>();

        [JsonProperty("successes")]
        public IList<Success> Successes { get; set; } = new List<Success>();
    }

    public class ResultDTO<T> : ResultDTO
    {
        [JsonProperty("value")]
        public T Value { get; set; }
    }
}