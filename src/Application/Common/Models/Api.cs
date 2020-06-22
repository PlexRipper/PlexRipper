using Newtonsoft.Json;
using PlexRipper.Application.Common.Interfaces.API;
using PlexRipper.Domain.Common.API;
using PlexRipper.Domain.Enums;
using PlexRipper.Domain.Extensions;
using Polly;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PlexRipper.Application.Common.Models
{
    public class Api : IApi
    {
        private Serilog.ILogger Log { get; }
        private readonly IPlexRipperHttpClient _plexRipperHttpClient;

        public Api(Serilog.ILogger log, IPlexRipperHttpClient plexRipperHttpClient)
        {
            Log = log;
            _plexRipperHttpClient = plexRipperHttpClient;
        }

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public async Task<T> Request<T>(Request request)
        {
            using (var httpRequestMessage = new HttpRequestMessage(request.HttpMethod, request.FullUri))
            {
                AddHeadersBody(request, httpRequestMessage);

                Log.Debug($"Api.Request => Sending request to: {httpRequestMessage.RequestUri}");
                HttpResponseMessage httpResponseMessage;

                try
                {
                    httpResponseMessage = await _plexRipperHttpClient.SendAsync(httpRequestMessage);
                }
                catch (Exception e)
                {
                    Log.Error(e, "An exception occured while sending a request", request);
                    throw;
                }

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
                                },
                                (exception, timeSpan, context) =>
                                {
                                    Log.Error(
                                        $"Retrying RequestUri: {request.FullUri} Because we got Status Code: {exception?.Result?.StatusCode}");
                                });

                        httpResponseMessage = await result.ExecuteAsync(async () =>
                        {
                            using (var req = await httpRequestMessage.Clone())
                            {
                                return await _plexRipperHttpClient.SendAsync(req);
                            }
                        });
                    }
                }
                var receivedString = await httpResponseMessage.Content.ReadAsStringAsync();

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    // do something with the response
                    Log.Verbose(receivedString);
                    if (request.ContentType == ContentType.Json)
                    {
                        try
                        {
                            request.OnBeforeDeserialization?.Invoke(receivedString);
                            return JsonConvert.DeserializeObject<T>(receivedString, Settings);
                        }
                        catch (JsonReaderException e)
                        {
                            Log.Error(e, $"Failed to parse Json object with type {typeof(T)}", receivedString);
                            throw;
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, $"Failed with Exception when parsing response with type {typeof(T)}", receivedString);
                            throw;
                        }

                    }
                    else
                    {
                        // XML
                        return DeserializeXml<T>(receivedString);
                    }
                }

                Log.Error("Failed to get a successfull response with request", request, receivedString);
            }

            return default;
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

                var httpResponseMessage = await _plexRipperHttpClient.SendAsync(httpRequestMessage);
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
                var httpResponseMessage = await _plexRipperHttpClient.SendAsync(httpRequestMessage);
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
                Log.Verbose("REQUEST: " + request.JsonBody);
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
            Log.Error($"StatusCode: {httpResponseMessage.StatusCode}, Reason: {httpResponseMessage.ReasonPhrase}, RequestUri: {request.FullUri}");
            await LogDebugContent(httpResponseMessage);
        }

        private async Task LogDebugContent(HttpResponseMessage message)
        {
            var content = await message.Content.ReadAsStringAsync();
            Log.Debug(content);
        }

    }
}
