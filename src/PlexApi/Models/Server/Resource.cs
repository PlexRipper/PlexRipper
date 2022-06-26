using System.Xml.Serialization;

namespace PlexRipper.PlexApi.Models;

public class Resource
{
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

    [XmlAttribute(AttributeName = "lastSeenAt")]
    public string LastSeenAt { get; set; }

    [XmlAttribute(AttributeName = "provides")]
    public string Provides { get; set; }

    [XmlAttribute(AttributeName = "owned")]
    public string Owned { get; set; }

    [XmlAttribute(AttributeName = "accessToken")]
    public string AccessToken { get; set; }

    [XmlAttribute(AttributeName = "publicAddress")]
    public string PublicAddress { get; set; }

    [XmlAttribute(AttributeName = "httpsRequired")]
    public string HttpsRequired { get; set; }

    [XmlAttribute(AttributeName = "synced")]
    public string Synced { get; set; }

    [XmlAttribute(AttributeName = "relay")]
    public string Relay { get; set; }

    [XmlAttribute(AttributeName = "dnsRebindingProtection")]
    public string DnsRebindingProtection { get; set; }

    [XmlAttribute(AttributeName = "natLoopbackSupported")]
    public string NatLoopbackSupported { get; set; }

    [XmlAttribute(AttributeName = "publicAddressMatches")]
    public string PublicAddressMatches { get; set; }

    [XmlAttribute(AttributeName = "presence")]
    public string Presence { get; set; }

    [XmlAttribute(AttributeName = "createdAt")]
    public string CreatedAt { get; set; }

    [XmlElement(ElementName = "Connection")]
    public List<Connection> Connections { get; set; }
}

public class Connection
{
    [XmlAttribute(AttributeName = "protocol")]
    public string Protocol { get; set; }

    [XmlAttribute(AttributeName = "address")]
    public string Address { get; set; }

    [XmlAttribute(AttributeName = "port")]
    public string Port { get; set; }

    [XmlAttribute(AttributeName = "uri")]
    public string Uri { get; set; }

    [XmlAttribute(AttributeName = "local")]
    public string Local { get; set; }
}