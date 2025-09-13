using System.Xml;

namespace Reaparr.PublicAPI.SearchTvShow;

using System.Xml.Serialization;

[XmlRoot("rss")]
public record TorznabMediaSearchResponseDTO
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "2.0";

    [XmlElement("channel")]
    public TorznabChannel Channel { get; set; } = new();

    // Declare namespaces so XmlSerializer knows about "torznab"
    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Xmlns { get; set; } = new(
    [
        new XmlQualifiedName("torznab", "http://torznab.com/schemas/2015/feed"),
    ]);
}

public record TorznabChannel
{
    [XmlElement("title")]
    public string Title { get; set; } = "PlexRipper Torznab";

    [XmlElement("description")]
    public string Description { get; set; } = "TV search results";

    [XmlElement("language")]
    public string Language { get; set; } = "en-us";   
    
    [XmlElement("category")]
    public string Category { get; set; } = "search";

    [XmlElement("item")]
    public List<TorznabItem> Items { get; set; } = new();
}

public record TorznabItem
{
    [XmlElement("title")]
    public string Title { get; set; } = string.Empty;

    [XmlElement("guid")]
    public TorznabGuid Guid { get; set; } = new();

    [XmlElement("link")]
    public string Link { get; set; } = string.Empty;

    [XmlElement("pubDate")]
    public string PubDate { get; set; } = DateTime.UtcNow.ToString("R");

    [XmlElement("size")]
    public long Size { get; set; }

    // Instead of torznab:attr, map to namespace
    [XmlElement("attr", Namespace = "http://torznab.com/schemas/2015/feed")]
    public List<TorznabAttr> Attributes { get; set; } = new();
    
    [XmlElement("enclosure")]
    public TorznabEnclosure Enclosure { get; set; } = new();
}

public record TorznabGuid
{
    [XmlAttribute("isPermaLink")]
    public string IsPermaLink { get; set; } = "false";

    [XmlText]
    public string Value { get; set; } = string.Empty;
}

public record TorznabAttr
{
    public TorznabAttr() { }

    public TorznabAttr(string name, string value)
    {
        Name = name;
        Value = value;
    }

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("value")]
    public string Value { get; set; } = string.Empty;
}

public record TorznabEnclosure
{
    [XmlAttribute("url")]
    public string Url { get; set; } = string.Empty;

    [XmlAttribute("length")]
    public long Length { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; } = "application/x-bittorrent";
}
