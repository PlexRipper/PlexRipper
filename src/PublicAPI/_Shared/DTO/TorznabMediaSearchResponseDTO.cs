namespace Reaparr.PublicAPI.SearchTvShow;

using System.Xml.Serialization;

[XmlRoot("rss")]
public class TorznabMediaSearchResponseDTO
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "2.0";

    [XmlElement("channel")]
    public TorznabChannel Channel { get; set; } = new();
}

public class TorznabChannel
{
    [XmlElement("title")]
    public string Title { get; set; } = "PlexRipper Torznab";

    [XmlElement("description")]
    public string Description { get; set; } = "TV search results";

    [XmlElement("language")]
    public string Language { get; set; } = "en-us";

    [XmlElement("item")]
    public List<TorznabItem> Items { get; set; } = new();
}

public class TorznabItem
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

    [XmlElement("torznab:attr")]
    public List<TorznabAttr> Attributes { get; set; } = new();
    
    [XmlElement("enclosure")]
    public TorznabEnclosure Enclosure { get; set; } = new();
}

public class TorznabGuid
{
    [XmlAttribute("isPermaLink")]
    public string IsPermaLink { get; set; } = "false";

    [XmlText]
    public string Value { get; set; } = string.Empty;
}

public class TorznabAttr
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("value")]
    public string Value { get; set; } = string.Empty;
}

public class TorznabEnclosure
{
    [XmlAttribute("url")]
    public string Url { get; set; } = string.Empty;

    [XmlAttribute("length")]
    public long Length { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; } = "application/x-bittorrent";
}

