using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PlexRipper.Application.Contracts;

public static class TorznabXmlFormatter
{
    public static string SerializeToXml<T>(T obj) where T : class
    {
        if (obj == null) return string.Empty;

        var xmlSerializer = new XmlSerializer(typeof(T));
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        
        // Add custom namespaces for Torznab
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("torznab", "http://torznab.com/schemas/2015/feed");
        
        xmlSerializer.Serialize(xmlWriter, obj, namespaces);
        return stringWriter.ToString();
    }

    public static TorznabCaps CreateDefaultCapabilities(string baseUrl, int maxResults = 100)
    {
        return new TorznabCaps
        {
            Server = new TorznabServer
            {
                Version = "1.0",
                Title = "PlexRipper",
                Strapline = "PlexRipper Torznab API - Download from Plex servers",
                Email = "",
                Url = baseUrl,
                Image = ""
            },
            Limits = new TorznabLimits
            {
                Max = maxResults,
                Default = Math.Min(100, maxResults)
            },
            Searching = new TorznabSearching
            {
                Search = new TorznabSearchCapability
                {
                    Available = "yes",
                    SupportedParams = "q"
                },
                TvSearch = new TorznabSearchCapability
                {
                    Available = "yes",
                    SupportedParams = "q,season,ep,imdbid,tvdbid"
                },
                MovieSearch = new TorznabSearchCapability
                {
                    Available = "yes",
                    SupportedParams = "q,imdbid,tmdbid"
                }
            },
            Categories = new List<TorznabCategory>
            {
                new() { Id = 2000, Name = "Movies" },
                new() { Id = 5000, Name = "TV" },
                new() { 
                    Id = 5030, 
                    Name = "TV/HD", 
                    SubCategories = new List<TorznabSubCategory>
                    {
                        new() { Id = 5040, Name = "TV/HD" }
                    }
                }
            }
        };
    }
}