using System.Xml.Serialization;

namespace PlexRipper.PlexApi.Models
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class FriendContainer
    {
        [XmlElement(ElementName = "User")]
        public Friend[] Friends { get; set; }

        [XmlAttribute(AttributeName = "friendlyName")]
        public string FriendlyName { get; set; }

        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }

        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }

        [XmlAttribute(AttributeName = "totalSize")]
        public string TotalSize { get; set; }

        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
    }
}