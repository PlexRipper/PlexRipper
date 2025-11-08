namespace Reaparr.Settings.Contracts;

public interface IRadarrSettings
{
    string BaseUrl { get; set; }

    string ApiKey { get; set; }
}
