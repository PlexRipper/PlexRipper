namespace PlexRipper.Application;

public interface IGeneralSettings
{
    bool FirstTimeSetup { get; set; }

    int ActiveAccountId { get; set; }
}