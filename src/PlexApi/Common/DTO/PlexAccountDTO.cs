using PlexRipper.PlexApi.Common.DTO.PlexGetServer;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO
{
    public class PlexAccountDTO
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("joined_at")]
        public DateTime JoinedAt { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("thumb")]
        public Uri Thumb { get; set; }

        [JsonPropertyName("hasPassword")]
        public bool HasPassword { get; set; }

        [JsonPropertyName("authToken")]
        public string AuthToken { get; set; }

        [JsonPropertyName("authentication_token")]
        public string AuthenticationToken { get; set; }

        [JsonPropertyName("subscription")]
        public SubscriptionDTO SubscriptionDto { get; set; }

        [JsonPropertyName("roles")]
        public RolesDTO RolesDto { get; set; }

        [JsonPropertyName("entitlements")]
        public List<object> Entitlements { get; set; }

        [JsonPropertyName("confirmedAt")]
        public object ConfirmedAt { get; set; }

        [JsonPropertyName("forumId")]
        public object ForumId { get; set; }

        [JsonPropertyName("rememberMe")]
        public bool RememberMe { get; set; }

        public List<PlexServerDTO> PlexServers { get; set; }
    }
}
