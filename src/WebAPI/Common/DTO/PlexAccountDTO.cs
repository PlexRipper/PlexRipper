using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.Application.Common.DTO.Plex;
using PlexRipper.Infrastructure.Common.DTO;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexAccountDTO
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("thumb")]
        public Uri Thumb { get; set; }

        [JsonProperty("hasPassword")]
        public bool HasPassword { get; set; }

        [JsonProperty("authToken")]
        public string AuthToken { get; set; }

        [JsonProperty("authentication_token")]
        public string AuthenticationToken { get; set; }

        [JsonProperty("subscription")]
        public SubscriptionDTO SubscriptionDto { get; set; }

        [JsonProperty("roles")]
        public RolesDTO RolesDto { get; set; }

        [JsonProperty("entitlements")]
        public List<object> Entitlements { get; set; }

        [JsonProperty("confirmedAt")]
        public object ConfirmedAt { get; set; }

        [JsonProperty("forumId")]
        public object ForumId { get; set; }

        [JsonProperty("rememberMe")]
        public bool RememberMe { get; set; }

        public List<PlexServerDTO> PlexServers { get; set; }
    }
}
