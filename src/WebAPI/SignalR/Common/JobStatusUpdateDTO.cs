using BackgroundServices.Contracts;
using Newtonsoft.Json;
using PlexRipper.WebAPI.Common;

namespace PlexRipper.WebAPI.SignalR.Common;

public class JobStatusUpdateDTO
{
    [JsonProperty("id", Required = Required.Always)]
    public string Id { get; set; }

    [JsonProperty("jobName", Required = Required.Always)]
    public string JobName { get; set; }

    [JsonProperty("jobGroup", Required = Required.Always)]
    public string JobGroup { get; set; }

    [JsonProperty("jobType", Required = Required.Always)]
    public JobTypes JobType { get; set; }

    [JsonProperty("jobRuntime", Required = Required.Always)]
    public TimeSpan JobRuntime { get; set; }

    [JsonProperty("jobStartTime", Required = Required.Always)]
    public DateTime JobStartTime { get; set; }

    [JsonProperty("status", Required = Required.Always)]
    public JobStatus Status { get; set; }

    [JsonProperty("primaryKey", Required = Required.Always)]
    public string PrimaryKey { get; set; }

    [JsonProperty("primaryKeyValue", Required = Required.Always)]
    public int PrimaryKeyValue { get; set; }
}