using FastEndpoints;

namespace Reaparr.PublicAPI.GetAllCategories;

public class GetAllCategoriesEndpoint : EndpointWithoutRequest<object>
{
    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/torrents/categories");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var categories = new Dictionary<string, object>
        {
            // This is the default category and avoids having to implement and track custom categories for Sonarr
            ["tv-sonarr"] = new { name = "tv-sonarr", savePath = "/home/user/torrents/video/" },
        };

        await Send.OkAsync(categories, cancellation: ct);
    }
}