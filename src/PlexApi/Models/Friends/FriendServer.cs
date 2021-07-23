using System.Xml.Serialization;

namespace PlexRipper.PlexApi.Models
{
    [XmlRoot(ElementName = "Server")]
    public class FriendServer
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "serverId")]
        public string ServerId { get; set; }

        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "lastSeenAt")]
        public string LastSeenAt { get; set; }

        [XmlAttribute(AttributeName = "numLibraries")]
        public string NumLibraries { get; set; }

        [XmlAttribute(AttributeName = "owned")]
        public string Owned { get; set; }
    }
}