using System.Xml.Serialization;

namespace PlexRipper.Application.Contracts;

[XmlRoot("rss")]
public class TorznabRss
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "2.0";

    [XmlElement("channel")]
    public TorznabChannel Channel { get; set; } = new();
}

public class TorznabChannel
{
    [XmlElement("title")]
    public string Title { get; set; } = "PlexRipper";

    [XmlElement("description")]
    public string Description { get; set; } = "PlexRipper Torznab API";

    [XmlElement("link")]
    public string Link { get; set; } = "";

    [XmlElement("language")]
    public string Language { get; set; } = "en-us";

    [XmlElement("item")]
    public List<TorznabItem> Items { get; set; } = new();
}

public class TorznabItem
{
    [XmlElement("title")]
    public string Title { get; set; } = "";

    [XmlElement("guid")]
    public string Guid { get; set; } = "";

    [XmlElement("link")]
    public string Link { get; set; } = "";

    [XmlElement("description")]
    public string Description { get; set; } = "";

    [XmlElement("pubDate")]
    public string PubDate { get; set; } = "";

    [XmlElement("size")]
    public long Size { get; set; }

    [XmlElement("enclosure")]
    public TorznabEnclosure Enclosure { get; set; } = new();

    [XmlArray("torznab:attr")]
    [XmlArrayItem("torznab:attribute")]
    public List<TorznabAttribute> Attributes { get; set; } = new();
}

public class TorznabEnclosure
{
    [XmlAttribute("url")]
    public string Url { get; set; } = "";

    [XmlAttribute("length")]
    public long Length { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; } = "application/x-bittorrent";
}

public class TorznabAttribute
{
    [XmlAttribute("name")]
    public string Name { get; set; } = "";

    [XmlAttribute("value")]
    public string Value { get; set; } = "";
}

[XmlRoot("caps")]
public class TorznabCaps
{
    [XmlElement("server")]
    public TorznabServer Server { get; set; } = new();

    [XmlElement("limits")]
    public TorznabLimits Limits { get; set; } = new();

    [XmlElement("searching")]
    public TorznabSearching Searching { get; set; } = new();

    [XmlArray("categories")]
    [XmlArrayItem("category")]
    public List<TorznabCategory> Categories { get; set; } = new();
}

public class TorznabServer
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "1.0";

    [XmlAttribute("title")]
    public string Title { get; set; } = "PlexRipper";

    [XmlAttribute("strapline")]
    public string Strapline { get; set; } = "PlexRipper Torznab API";

    [XmlAttribute("email")]
    public string Email { get; set; } = "";

    [XmlAttribute("url")]
    public string Url { get; set; } = "";

    [XmlAttribute("image")]
    public string Image { get; set; } = "";
}

public class TorznabLimits
{
    [XmlAttribute("max")]
    public int Max { get; set; } = 100;

    [XmlAttribute("default")]
    public int Default { get; set; } = 100;
}

public class TorznabSearching
{
    [XmlElement("search")]
    public TorznabSearchCapability Search { get; set; } = new();

    [XmlElement("tv-search")]
    public TorznabSearchCapability TvSearch { get; set; } = new();

    [XmlElement("movie-search")]
    public TorznabSearchCapability MovieSearch { get; set; } = new();
}

public class TorznabSearchCapability
{
    [XmlAttribute("available")]
    public string Available { get; set; } = "yes";

    [XmlAttribute("supportedParams")]
    public string SupportedParams { get; set; } = "";
}

public class TorznabCategory
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; } = "";

    [XmlElement("subcat")]
    public List<TorznabSubCategory> SubCategories { get; set; } = new();
}

public class TorznabSubCategory
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; } = "";
}