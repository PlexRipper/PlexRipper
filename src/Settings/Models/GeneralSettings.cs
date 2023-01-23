using PlexRipper.Application;
using Settings.Contracts;

namespace PlexRipper.Settings.Models;

public class GeneralSettings : IGeneralSettings
{
    public bool FirstTimeSetup { get; set; }

    public int ActiveAccountId { get; set; }
}