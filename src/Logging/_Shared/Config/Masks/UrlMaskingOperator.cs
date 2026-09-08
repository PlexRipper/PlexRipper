using System.Text.RegularExpressions;
using Serilog.Enrichers.Sensitive;

namespace Reaparr.Logging;

public class UrlMaskingOperator : RegexMaskingOperator
{
    public UrlMaskingOperator()
        : base(STATUS_URL_REPLACE_PATTERN) { }

    private const string STATUS_URL_REPLACE_PATTERN =
        @"^(https?://)(?:www\.)?([a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*|\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})(?::\d+)?(?:/[^\s]*)?$";

    protected override string PreprocessMask(string mask, Match match)
    {
        try
        {
            var url = new Uri(match.Value);
            var query = url.Query;
            if (query.Length == 0)
                return $"{url.Scheme}://{mask}{url.AbsolutePath}";

            var queryParts = query[1..].Split('&');
            for (var i = 0; i < queryParts.Length; i++)
            {
                var separatorIndex = queryParts[i].IndexOf('=');
                queryParts[i] = separatorIndex < 0 ? mask : $"{queryParts[i][..(separatorIndex + 1)]}{mask}";
            }

            return $"{url.Scheme}://{mask}{url.AbsolutePath}?{string.Join('&', queryParts)}";
        }
        catch (Exception)
        {
            return match.Value;
        }
    }
}
