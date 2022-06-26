using System.Xml.Serialization;

namespace PlexRipper.PlexApi.Models
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class ResourceContainer
    {
        [XmlElement(ElementName = "Device")]
        public List<Resource> Devices { get; set; }
    }
}