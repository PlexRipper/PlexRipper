using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Photino.NET;

namespace Reaparr.AppHost;

/// <summary>Manages the Photino desktop window lifecycle.</summary>
public class DesktopMode : IDesktopMode
{
    private readonly Serilog.ILogger _log;
    private readonly IServer _server;

    /// <summary>Initializes a new instance of <see cref="DesktopMode"/>.</summary>
    public DesktopMode(Serilog.ILogger log, IServer server)
    {
        _log = log.ForContext<DesktopMode>();
        _server = server;
    }

    /// <inheritdoc />
    public Result Setup()
    {
        _log.Information("Starting DesktopMode");

        Uri uri;
        if (EnvironmentExtensions.IsDevelopmentEnvironment())
        {
            uri = new Uri("http://localhost:3000");
        }
        else
        {
            var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
            var serverAddress = serverAddressesFeature?.Addresses.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(serverAddress))
                return Result.Fail("Desktop mode could not determine the server address for the embedded window.");

            uri = new Uri(serverAddress);
        }

        // Creating a new PhotinoWindow instance with the fluent API
        var window = new PhotinoWindow()
            .SetTitle("Reaparr")
            .SetUseOsDefaultSize(true)
            .Center()
            .SetMinSize(1920, 1080)
            .SetMaximized(true)
            .SetResizable(true)
            .SetLogVerbosity(0)
            .Load(uri);

        window.WaitForClose(); // Starts the application event loop

        return Result.Ok();
    }
}
