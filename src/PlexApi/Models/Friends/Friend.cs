using System.Xml.Serialization;

namespace PlexRipper.PlexApi.Models.Friends
{
    [XmlRoot(ElementName = "User")]
    public class Friend
    {
        [XmlElement(ElementName = "Server")]
        public FriendServer Server { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "username")]
        public string Username { get; set; }

        [XmlAttribute(AttributeName = "email")]
        public string Email { get; set; }

        [XmlAttribute(AttributeName = "recommendationsPlaylistId")]
        public string RecommendationsPlaylistId { get; set; }

        [XmlAttribute(AttributeName = "thumb")]
        public string Thumb { get; set; }
    }
}