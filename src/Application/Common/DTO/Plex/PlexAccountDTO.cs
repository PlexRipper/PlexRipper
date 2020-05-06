using Newtonsoft.Json;
using PlexRipper.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace PlexRipper.Application.Common.DTO.Plex
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
        public Subscription Subscription { get; set; }

        [JsonProperty("roles")]
        public Roles Roles { get; set; }

        [JsonProperty("entitlements")]
        public List<object> Entitlements { get; set; }

        [JsonProperty("confirmedAt")]
        public object ConfirmedAt { get; set; }

        [JsonProperty("forumId")]
        public object ForumId { get; set; }

        [JsonProperty("rememberMe")]
        public bool RememberMe { get; set; }
    }
}
