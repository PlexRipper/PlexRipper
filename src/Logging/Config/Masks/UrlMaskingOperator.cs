using System.Text.RegularExpressions;
using System.Web;
using Serilog.Enrichers.Sensitive;

namespace Logging.Masks;

public class UrlMaskingOperator : RegexMaskingOperator
{
    #region Constructors

    public UrlMaskingOperator() : base(urlReplacePattern) { }

    #endregion

    private const string urlReplacePattern =
        @"^(https?://)(?:www\.)?([a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*|\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})(?::\d+)?(?:/[^\s]*)?$";

    protected override string PreprocessMask(string mask, Match match)
    {
        try
        {
            var url = new Uri(match.Value);
            var token = HttpUtility.ParseQueryString(url.Query).Get("X-Plex-Token") ?? "None123";
            return $"{url.Scheme}://{mask}{url.PathAndQuery.Replace(token, mask)}";
        }
        catch (Exception e)
        {
            return match.Value;
        }
    }
}