using System;
using System.Xml.Serialization;
using Plex.Api;

namespace PlexRipper.PlexApi.Models.Server
{
    /// <summary>
    /// 
    /// </summary>
    public class Server
    {
        [XmlAttribute(AttributeName = "accessToken")]
        public string AccessToken { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "address")]
        public string Address { get; set; }
        [XmlAttribute(AttributeName = "port")]
        public string Port { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "scheme")]
        public string Scheme { get; set; }
        [XmlAttribute(AttributeName = "host")]
        public string Host { get; set; }
        [XmlAttribute(AttributeName = "localAddresses")]
        public string LocalAddresses { get; set; }
        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }
        [XmlAttribute(AttributeName = "createdAt")]
        public string CreatedAt { get; set; }
        [XmlAttribute(AttributeName = "updatedAt")]
        public string UpdatedAt { get; set; }
        [XmlAttribute(AttributeName = "owned")]
        public string Owned { get; set; }
        [XmlAttribute(AttributeName = "synced")]
        public string Synced { get; set; }
        [XmlAttribute(AttributeName = "sourceTitle")]
        public string SourceTitle { get; set; }
        [XmlAttribute(AttributeName = "ownerId")]
        public string OwnerId { get; set; }
        [XmlAttribute(AttributeName = "home")]
        public string Home { get; set; }

        public Uri FullUri => this.Host.ReturnUriFromServerInfo(this);
    }
}