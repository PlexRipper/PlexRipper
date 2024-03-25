namespace Application.Contracts;

public static class PlexMediaDTOExtensions
{
    public static void SetFullThumbnailUrl(this PlexMediaDTO plexMediaSlim, string connectionUrl, string plexServerToken)
    {
        if (connectionUrl == string.Empty || (plexMediaSlim).FullThumbUrl == string.Empty || plexServerToken == string.Empty)
        {
            plexMediaSlim.HasThumb = false;
            return;
        }

        var uri = new Uri(connectionUrl + plexMediaSlim.FullThumbUrl);
        plexMediaSlim.FullThumbUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}/photo/:/transcode?url={uri.AbsolutePath}&X-Plex-Token={plexServerToken}";
        plexMediaSlim.HasThumb = true;
    }
}