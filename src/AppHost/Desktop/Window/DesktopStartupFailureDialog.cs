using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using Photino.NET;

namespace Reaparr.AppHost;

/// <summary>
/// Displays a diagnostic startup-failure dialog for desktop mode when Reaparr cannot boot.
/// </summary>
public class DesktopStartupFailureDialog
{
    private readonly IPathProvider _pathProvider;
    private readonly IAppBuildInfo _appBuildInfo;
    private readonly ILogBufferService _logBufferService;
    private const string TEMPLATE_RESOURCE_SUFFIX = "Desktop.Window.Templates.DesktopStartupFailureDialog.html";

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopStartupFailureDialog"/> class.
    /// </summary>
    /// <param name="appBuildInfo">Build metadata used for diagnostic display content.</param>
    /// <param name="pathProvider">Path provider used to resolve the logs directory.</param>
    /// <param name="logBufferService">In-memory log buffer used to render recent log lines in the dialog.</param>
    public DesktopStartupFailureDialog(
        IAppBuildInfo appBuildInfo,
        IPathProvider pathProvider,
        ILogBufferService logBufferService
    )
    {
        _pathProvider = pathProvider;
        _appBuildInfo = appBuildInfo;
        _logBufferService = logBufferService;
    }

    /// <summary>
    /// Shows the startup-failure diagnostic dialog populated from the provided failed result.
    /// </summary>
    /// <param name="errorResult">The failed result containing startup error information.</param>
    public void Show(Result errorResult)
    {
        var safeLogsDirectory = _pathProvider.LogsDirectory;
        var details = BuildDetails(errorResult);
        IReadOnlyCollection<string> logContent = _logBufferService
            .GetAll()
            .Select(x => x.ToString())
            .ToList();
        var html = BuildHtmlFromTemplate(details, logContent);

        var window = new PhotinoWindow()
            .SetTitle("Reaparr failed to start")
            .SetMinSize(1400, 900)
            .SetMaximized(true)
            .Center()
            .SetResizable(true)
            .SetLogVerbosity(2)
            .LoadRawString(html);

        window.RegisterWebMessageReceivedHandler(
            (_, message) =>
            {
                switch (message)
                {
                    case "open-logs":
                        TryOpenLogsWithFallbacks(safeLogsDirectory);
                        break;
                    case "open-website":
                        TryOpenPath("https://www.reaparr.rocks/");
                        break;
                }
            }
        );

        window.RegisterWindowClosingHandler(
            (_, _) =>
            {
                System.Environment.Exit(1);
                return false;
            }
        );

        window.WaitForClose();
    }

    private static void TryOpenLogsWithFallbacks(string logsDirectory)
    {
        if (!Directory.Exists(logsDirectory))
        {
            return;
        }

        if (TryOpenPath(logsDirectory))
        {
            return;
        }

        if (OperatingSystem.IsLinux())
        {
            if (TryOpenCommand("xdg-open", logsDirectory))
            {
                return;
            }

            if (TryOpenCommand("gio", $"open \"{logsDirectory}\""))
            {
                return;
            }
        }

        var fallbackUri = new Uri(
            "file://" + logsDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar
        );
        TryOpenPath(fallbackUri.AbsoluteUri);
    }

    private static bool TryOpenCommand(string command, string argument)
    {
        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = argument,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            );
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryOpenPath(string candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = candidate, UseShellExecute = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string BuildDetails(Result result)
    {
        var firstError = result.Errors.FirstOrDefault();
        var exception = (firstError as ExceptionalError)?.Exception;

        var builder = new StringBuilder();
        builder.AppendLine($"Time (UTC): {DateTime.UtcNow:O}");
        builder.AppendLine($"Version: {_appBuildInfo.InformationalVersion}");
        builder.AppendLine($"Logs directory: {_pathProvider.LogsDirectory}");
        builder.AppendLine($"Current directory: {System.Environment.CurrentDirectory}");
        builder.AppendLine($"Base directory: {AppContext.BaseDirectory}");
        builder.AppendLine($"Process architecture: {RuntimeInformation.ProcessArchitecture}");
        builder.AppendLine($"OS: {RuntimeInformation.OSDescription}");
        builder.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");

        if (exception is not null)
        {
            builder.AppendLine($"Exception: {exception.GetType().FullName}");
            builder.AppendLine($"Message: {exception.Message}");
            builder.AppendLine();
            builder.AppendLine(exception.ToString());
        }
        else
        {
            builder.AppendLine($"Error: {firstError?.Message ?? "Unknown startup failure"}");
            builder.AppendLine();
            builder.AppendLine(result.ToString());
        }

        return builder.ToString();
    }

    private string BuildHtmlFromTemplate(string details, IReadOnlyCollection<string> logContent)
    {
        var template = LoadTemplate();

        return template
            .Replace("{{VERSION}}", HtmlEncoder.Default.Encode(_appBuildInfo.InformationalVersion))
            .Replace("{{LOGS_PATH}}", HtmlEncoder.Default.Encode(_pathProvider.LogsDirectory))
            .Replace("{{DETAILS}}", HtmlEncoder.Default.Encode(details))
            .Replace(
                "{{LOG_CONTENT}}",
                HtmlEncoder.Default.Encode(string.Join(System.Environment.NewLine, logContent))
            );
    }

    private static string LoadTemplate()
    {
        var assembly = typeof(DesktopStartupFailureDialog).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(TEMPLATE_RESOURCE_SUFFIX, StringComparison.Ordinal));

        if (resourceName is null)
        {
            return "<!doctype html><html><body><h1>Reaparr failed to start</h1><pre>Startup failure template resource not found.</pre></body></html>";
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return "<!doctype html><html><body><h1>Reaparr failed to start</h1><pre>Startup failure template stream could not be opened.</pre></body></html>";
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
