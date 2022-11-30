using System.Xml.Serialization;

namespace PlexRipper.PlexApi.Api;

// Used for retrieving servers by plex account token
// e.g: https://plex.tv/api/v2/resources?X-Plex-Token=XXXXXXXXXXXX&X-Plex-Client-Identifier=chrome

[XmlRoot(ElementName = "resources")]
public class Resources
{
    [XmlElement(ElementName = "resource")]
    public List<ServerResource> Resource { get; set; }
}

[XmlRoot(ElementName = "connection")]
public class ServerConnection
{
    [XmlAttribute(AttributeName = "protocol")]
    public string Protocol { get; set; }

    [XmlAttribute(AttributeName = "address")]
    public string Address { get; set; }

    [XmlAttribute(AttributeName = "port")]
    public int Port { get; set; }

    [XmlAttribute(AttributeName = "uri")]
    public string Uri { get; set; }

    [XmlAttribute(AttributeName = "local")]
    public int Local { get; set; }

    [XmlAttribute(AttributeName = "relay")]
    public int Relay { get; set; }

    [XmlAttribute(AttributeName = "IPv6")]
    public int IPv6 { get; set; }
}

[XmlRoot(ElementName = "connections")]
public class Connections
{
    [XmlElement(ElementName = "connection")]
    public List<ServerConnection> Connection { get; set; }
}

[XmlRoot(ElementName = "resource")]
public class ServerResource
{
    [XmlElement(ElementName = "connections")]
    public Connections Connections { get; set; }

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }

    [XmlAttribute(AttributeName = "product")]
    public string Product { get; set; }

    [XmlAttribute(AttributeName = "productVersion")]
    public string ProductVersion { get; set; }

    [XmlAttribute(AttributeName = "platform")]
    public string Platform { get; set; }

    [XmlAttribute(AttributeName = "platformVersion")]
    public string PlatformVersion { get; set; }

    [XmlAttribute(AttributeName = "device")]
    public string Device { get; set; }

    [XmlAttribute(AttributeName = "clientIdentifier")]
    public string ClientIdentifier { get; set; }

    [XmlAttribute(AttributeName = "createdAt")]
    public DateTime CreatedAt { get; set; }

    [XmlAttribute(AttributeName = "lastSeenAt")]
    public DateTime LastSeenAt { get; set; }

    [XmlAttribute(AttributeName = "provides")]
    public string Provides { get; set; }

    [XmlAttribute(AttributeName = "ownerId")]
    public object OwnerId { get; set; }

    [XmlAttribute(AttributeName = "sourceTitle")]
    public object SourceTitle { get; set; }

    [XmlAttribute(AttributeName = "publicAddress")]
    public string PublicAddress { get; set; }

    [XmlAttribute(AttributeName = "accessToken")]
    public string AccessToken { get; set; }

    [XmlAttribute(AttributeName = "owned")]
    public int Owned { get; set; }

    [XmlAttribute(AttributeName = "home")]
    public int Home { get; set; }

    [XmlAttribute(AttributeName = "synced")]
    public int Synced { get; set; }

    [XmlAttribute(AttributeName = "relay")]
    public int Relay { get; set; }

    [XmlAttribute(AttributeName = "presence")]
    public int Presence { get; set; }

    [XmlAttribute(AttributeName = "httpsRequired")]
    public int HttpsRequired { get; set; }

    [XmlAttribute(AttributeName = "publicAddressMatches")]
    public int PublicAddressMatches { get; set; }

    [XmlAttribute(AttributeName = "dnsRebindingProtection")]
    public int DnsRebindingProtection { get; set; }

    [XmlAttribute(AttributeName = "natLoopbackSupported")]
    public int NatLoopbackSupported { get; set; }
}