namespace Reaparr.Settings.Contracts;

public interface ISonarrSettings
{
    string BaseUrl { get; set; }
    string ApiKey { get; set; }
}
