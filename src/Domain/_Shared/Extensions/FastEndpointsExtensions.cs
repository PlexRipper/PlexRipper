using System.Xml.Serialization;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;

namespace Reaparr.Domain;

public static class FastEndpointsExtensions
{
    public static TBuilder IsInternalApi<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithGroupName("Internal API");
        return builder;
    }

    public static TBuilder IsPublicApi<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithGroupName("Public API");
        return builder;
    }

    public static TBuilder IsDownloadClient<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithTags("Download Client");
        return builder;
    }

    public static TBuilder IsIndexer<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithTags("Indexer");
        return builder;
    }

    public static async Task XMLAsync<TResponse>(
        this IResponseSender ep,
        TResponse response,
        int statusCode = 200,
        string contentType = "application/xml",
        CancellationToken cancellationToken = default
    )
    {
        ep.HttpContext.MarkResponseStart();
        ep.HttpContext.Response.StatusCode = statusCode;
        ep.HttpContext.Response.ContentType = contentType;
        var xmlSerializer = new XmlSerializer(typeof(TResponse));
        using var stream = new MemoryStream();
        xmlSerializer.Serialize(stream, response);
        stream.Position = 0;
        await stream.CopyToAsync(ep.HttpContext.Response.Body, cancellationToken);
    }
}
