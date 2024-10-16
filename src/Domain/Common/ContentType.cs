using System.Net.Http.Headers;

namespace PlexRipper.Domain;

public static class ContentType
{
    public static string ApplicationJson => "application/json";

    public static string TextHtml => "text/html";

    public static MediaTypeWithQualityHeaderValue ApplicationJsonHeaderValue => new(ApplicationJson);
}
