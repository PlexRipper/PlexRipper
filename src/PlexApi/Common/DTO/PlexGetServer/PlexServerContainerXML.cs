using System.Collections.Generic;
using System.Xml.Serialization;

namespace PlexRipper.PlexApi.Common.DTO.PlexGetServer
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class PlexServerContainerXML
    {
        [XmlElement(ElementName = "Server")]
        public List<PlexServerXML> Server { get; set; }
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
