﻿using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO;

public class InspectServerProgressDTO
{
    [JsonProperty("plexServerId", Required = Required.Always)]
    public int PlexServerId { get; set; }

    [JsonProperty("retryAttemptIndex", Required = Required.Always)]
    public int RetryAttemptIndex { get; set; }

    [JsonProperty("retryAttemptCount", Required = Required.Always)]
    public int RetryAttemptCount { get; set; }

    [JsonProperty("timeToNextRetry", Required = Required.Always)]
    public int TimeToNextRetry { get; set; }

    [JsonProperty("statusCode", Required = Required.Always)]
    public int StatusCode { get; set; }

    [JsonProperty("connectionSuccessful", Required = Required.Always)]
    public bool ConnectionSuccessful { get; set; }

    [JsonProperty("completed", Required = Required.Always)]
    public bool Completed { get; set; }

    [JsonProperty("message", Required = Required.Always)]
    public string Message { get; set; }

    [JsonProperty("plexServerConnection", Required = Required.Always)]
    public PlexServerConnectionDTO PlexServerConnection { get; set; }
}