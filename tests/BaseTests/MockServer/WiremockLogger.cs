using Logging.Interface;
using WireMock.Admin.Requests;
using WireMock.Logging;

namespace PlexRipper.BaseTests;

public class WiremockLogger : IWireMockLogger
{
    private readonly ILog _log;

    public WiremockLogger(ILog log)
    {
        _log = log;
    }

    public void Debug(string formatString, params object[] args)
    {
        _log.Debug(formatString, args);
    }

    public void Info(string formatString, params object[] args)
    {
        _log.Information(formatString, args);
    }

    public void Warn(string formatString, params object[] args)
    {
        _log.Warning(formatString, args);

        // TODO see https://github.com/WireMock-Net/WireMock.Net/pull/1182
        if (formatString.Contains("No matching mapping found"))
            throw new Exception(formatString);
    }

    public void Error(string formatString, params object[] args)
    {
        _log.Error(formatString, args);
        throw new Exception(formatString);
    }

    public void Error(string formatString, Exception exception)
    {
        _log.Error(exception);
        throw exception;
    }

    public void DebugRequestResponse(LogEntryModel logEntryModel, bool isAdminRequest)
    {
        _log.DebugLine(logEntryModel.ToString() ?? string.Empty);
    }
}