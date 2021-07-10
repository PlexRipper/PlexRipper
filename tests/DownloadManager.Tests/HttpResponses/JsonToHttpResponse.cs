using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace DownloadManager.Tests.HttpResponses
{
    public static class JsonToHttpResponse
    {
        public static HttpResponseMessage GetDownloadHeaderResponse()
        {
            var jsonString = File.ReadAllText(@".\HttpResponses\DownloadHeader.json");
            return JsonConvert.DeserializeObject<HttpResponseMessage>(jsonString);
        }
    }
}