using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using Photino.NET;

namespace Reaparr.AppHost;

internal static class DesktopStartupFailureDialog
{
    public static void Show(Result result, string? logsDirectory, string? appVersion)
    {
        var safeLogsDirectory = string.IsNullOrWhiteSpace(logsDirectory) ? "(unknown)" : logsDirectory;

        var details = BuildDetails(result, safeLogsDirectory, appVersion);
        var html = BuildHtml(details);

        var window = new PhotinoWindow()
            .SetTitle("Reaparr failed to start")
            .SetUseOsDefaultSize(true)
            .Center()
            .SetResizable(true)
            .SetLogVerbosity(2)
            .LoadRawString(html);

        window.RegisterWebMessageReceivedHandler(
            (_, message) =>
            {
                if (message == "open-logs" && Directory.Exists(safeLogsDirectory))
                {
                    Process.Start(new ProcessStartInfo { FileName = safeLogsDirectory, UseShellExecute = true });
                }
                else if (message == "exit")
                {
                    window.Close();
                }
            }
        );

        window.WaitForClose();
    }

    private static string BuildDetails(Result result, string logsDirectory, string? appVersion)
    {
        var firstError = result.Errors.FirstOrDefault();
        var exception = (firstError as ExceptionalError)?.Exception;

        var builder = new StringBuilder();
        builder.AppendLine($"Time (UTC): {DateTime.UtcNow:O}");
        builder.AppendLine($"Version: {appVersion ?? "unknown"}");
        builder.AppendLine($"Logs: {logsDirectory}");

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

    private static string BuildHtml(string details)
    {
        var encoded = HtmlEncoder.Default.Encode(details);

        return $$"""
            <!doctype html>
            <html lang=\"en\">
            <head>
              <meta charset=\"utf-8\" />
              <title>Reaparr failed to start</title>
              <style>
                body { font-family: Segoe UI, Arial, sans-serif; margin: 16px; background: #111; color: #eee; }
                h1 { margin-top: 0; color: #ff6b6b; }
                button { margin-right: 8px; padding: 8px 12px; border: 0; border-radius: 6px; cursor: pointer; }
                .secondary { background: #2a2a2a; color: #fff; }
                .primary { background: #b00020; color: #fff; }
                pre { background: #1a1a1a; padding: 12px; border-radius: 8px; overflow: auto; white-space: pre-wrap; }
              </style>
            </head>
            <body>
              <h1>Reaparr failed to start</h1>
              <p>The desktop app encountered an error during startup. Please copy details and share logs.</p>
              <div>
                <button class=\"secondary\" onclick=\"window.external.sendMessage('open-logs')\">Open Logs Folder</button>
                <button class=\"primary\" onclick=\"window.external.sendMessage('exit')\">Exit</button>
              </div>
              <h2>Details</h2>
              <pre>{{encoded}}</pre>
            </body>
            </html>
            """;
    }
}
