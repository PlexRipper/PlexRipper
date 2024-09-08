using System.Net.Http.Headers;
using HttpClientToCurl;
using LukeHagar.PlexAPI.SDK.Utils;
using Serilog.Events;
using ILog = Logging.Interface.ILog;

namespace PlexRipper.PlexApi;

public class NewPlexApiClient : ISpeakeasyHttpClient
{
    private readonly ILog _log;

    private readonly HttpClient _defaultClient;

    public NewPlexApiClient(ILog log)
    {
        _log = log;
        _defaultClient = new HttpClient
        {
            DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
        };
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        if (_log.IsLogLevelEnabled(LogEventLevel.Verbose))
        {
            var curl = _defaultClient.GenerateCurlInString(request);
            _log.Verbose("Request CURL: {RequestUrl}", curl);
        }
        else
            _log.Debug("Request: {RequestUrl}", request.RequestUri.ToString());

        return _defaultClient.SendAsync(request);
    }

    public Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request) => throw new NotImplementedException();
}
