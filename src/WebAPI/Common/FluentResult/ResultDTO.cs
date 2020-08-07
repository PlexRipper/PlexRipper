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
        public IList<Reason> Reasons { get; set; }

        [JsonProperty("errors")]
        public IList<Error> Errors { get; set; }

        [JsonProperty("successes")]
        public IList<Success> Successes { get; set; }
    }

    public class ResultDTO<T> : ResultDTO
    {
        [JsonProperty("value")]
        public T Value { get; set; }
    }
}