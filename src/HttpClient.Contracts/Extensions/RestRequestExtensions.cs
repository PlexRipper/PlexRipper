using System.Net.Http.Headers;
using RestSharp;

namespace HttpClient.Contracts;

public static class RestRequestExtensions
{
    public static void AddRangeHeader(this RestRequest request, RangeHeaderValue headerValue)
    {
        request.AddHeader("Range", headerValue.ToString());
    }
}
