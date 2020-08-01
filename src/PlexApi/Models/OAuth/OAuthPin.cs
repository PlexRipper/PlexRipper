using System;

namespace PlexRipper.PlexApi.Models.OAuth
{
    public class OAuthPin
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public bool Trusted { get; set; }
        public string ClientIdentifier { get; set; }
        public Location Location { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string AuthToken { get; set; }
        public string Url { get; set; }
    }

    public class Location
    {
        public string Code { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Subdivisions { get; set; }
        public string Coordinates { get; set; }
    }

}