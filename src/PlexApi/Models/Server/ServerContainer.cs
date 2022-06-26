using System.Xml.Serialization;

namespace PlexRipper.PlexApi.Models
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class ServerContainer
    {
        [XmlElement(ElementName = "Server")]
        public List<Server> Servers { get; set; }

        [XmlAttribute(AttributeName = "friendlyName")]
        public string FriendlyName { get; set; }

        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }

        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }

        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
    }
}