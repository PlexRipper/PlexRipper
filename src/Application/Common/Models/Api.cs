using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlexRipper.Application.Common.Interfaces.API;
using PlexRipper.Domain.Common.API;
using PlexRipper.Domain.Enums;
using PlexRipper.Domain.Extensions;
using Polly;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PlexRipper.Application.Common.Models
{
    public class Api : IApi
    {
        public Api(ILogger<Api> log, IPlexRipperHttpClient client)
        {
            Logger = log;
            _client = client;
        }

        private ILogger<Api> Logger { get; }
        private readonly IPlexRipperHttpClient _client;

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public async Task<bool> Download(Request request, string fileName)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFileCompleted += (sender, args) =>
                    {
                        Logger.LogInformation("The download has completed!");
                    };

                    // Specify a progress notification handler.
                    webClient.DownloadProgressChanged += (sender, args) =>
                    {
                        Logger.LogInformation($"Downloaded {args.BytesReceived} of {args.TotalBytesToReceive} bytes. {args.ProgressPercentage} % complete...");
                    };

                    string downloadPath = @$"{Environment.CurrentDirectory}\PlexDownloads2\{fileName}";
                    Task.WaitAll(webClient.DownloadFileTaskAsync(request.FullUri, @downloadPath));
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to download File: {fileName}", e);
            }
            return false;
        }

        public async Task<T> Request<T>(Request request)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);

                Logger.LogDebug($"Sending request to: ${httpRequestMessage.RequestUri}");

                var httpResponseMessage = await _client.SendAsync(httpRequestMessage);

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (!request.IgnoreErrors)
                    {
                        await LogError(request, httpResponseMessage);
                    }

                    if (request.Retry)
                    {

                        var result = Policy
                            .Handle<HttpRequestException>()
                            .OrResult<HttpResponseMessage>(r => request.StatusCodeToRetry.Contains(r.StatusCode))
                            .WaitAndRetryAsync(new[]
                            {
                                TimeSpan.FromSeconds(10),
                            }, (exception, timeSpan, context) =>
                            {

                                Logger.LogError($"Retrying RequestUri: {request.FullUri} Because we got Status Code: {exception?.Result?.StatusCode}");
                            });

                        httpResponseMessage = await result.ExecuteAsync(async () =>
                        {
                            using (var req = await httpRequestMessage.Clone())
                            {
                                return await _client.SendAsync(req);
                            }
                        });
                    }
                }

                // do something with the response
                var receivedString = await httpResponseMessage.Content.ReadAsStringAsync();
                LogDebugContent(receivedString);
                if (request.ContentType == ContentType.Json)
                {
                    request.OnBeforeDeserialization?.Invoke(receivedString);
                    return JsonConvert.DeserializeObject<T>(receivedString, Settings);
                }
                else
                {
                    // XML
                    return DeserializeXml<T>(receivedString);
                }
            }

        }

        public T DeserializeXml<T>(string receivedString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(receivedString);
            var value = (T)serializer.Deserialize(reader);
            return value;
        }

        public async Task<string> RequestContent(Request request)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);

                var httpResponseMessage = await _client.SendAsync(httpRequestMessage);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (!request.IgnoreErrors)
                    {
                        await LogError(request, httpResponseMessage);
                    }
                }
                // do something with the response
                var data = httpResponseMessage.Content;
                await LogDebugContent(httpResponseMessage);
                return await data.ReadAsStringAsync();
            }

        }

        public async Task Request(Request request)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);
                var httpResponseMessage = await _client.SendAsync(httpRequestMessage);
                await LogDebugContent(httpResponseMessage);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (!request.IgnoreErrors)
                    {
                        await LogError(request, httpResponseMessage);
                    }
                }
            }
        }

        private void AddHeadersBody(Request request, HttpRequestMessage httpRequestMessage)
        {
            // Add the Json Body
            if (request.JsonBody != null)
            {
                LogDebugContent("REQUEST: " + request.JsonBody);
                httpRequestMessage.Content = new JsonContent(request.JsonBody);
                httpRequestMessage.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json"); // Emby connect fails if we have the charset in the header
            }

            // Add headers
            foreach (var header in request.Headers)
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }
        }

        private async Task LogError(Request request, HttpResponseMessage httpResponseMessage)
        {
            Logger.LogError($"StatusCode: {httpResponseMessage.StatusCode}, Reason: {httpResponseMessage.ReasonPhrase}, RequestUri: {request.FullUri}");
            await LogDebugContent(httpResponseMessage);
        }

        private async Task LogDebugContent(HttpResponseMessage message)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                var content = await message.Content.ReadAsStringAsync();
                Logger.LogDebug(content);
            }
        }

        private void LogDebugContent(string message)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug(message);
            }
        }
    }
}
