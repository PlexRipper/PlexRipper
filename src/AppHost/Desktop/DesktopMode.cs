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

        var uri = EnvironmentExtensions.IsDevelopmentEnvironment()
            ? new Uri("http://localhost:3000")
            : _server.Features.Get<IServerAddressesFeature>()!.Addresses.Select(a => new Uri(a)).First();

        // Creating a new PhotinoWindow instance with the fluent API
        var window = new PhotinoWindow()
            .SetTitle("Reaparr")
            .SetUseOsDefaultSize(true)
            .Center()
            .SetResizable(true)
            .Load(uri);

        window.WaitForClose(); // Starts the application event loop

        return Result.Ok();
    }
}
