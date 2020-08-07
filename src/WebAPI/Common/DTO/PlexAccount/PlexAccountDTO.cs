using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexAccountDTO
    {
        [JsonProperty("id", Required = Required.DisallowNull)]
        public int Id { get; set; }

        [JsonProperty("displayName", Required = Required.DisallowNull)]
        public string DisplayName { get; set; }

        [JsonProperty("username", Required = Required.DisallowNull)]
        public string Username { get; set; }

        [JsonProperty("password", Required = Required.DisallowNull)]
        public string Password { get; set; }

        [JsonProperty("isEnabled", Required = Required.DisallowNull)]
        public bool IsEnabled { get; set; }

        [JsonProperty("isValidated", Required = Required.DisallowNull)]
        public bool IsValidated { get; set; }

        [JsonProperty("validatedAt", Required = Required.DisallowNull)]
        public DateTime ValidatedAt { get; set; }

        [JsonProperty("uuid", Required = Required.DisallowNull)]
        public string Uuid { get; set; }

        [JsonProperty("email", Required = Required.DisallowNull)]
        public string Email { get; set; }

        [JsonProperty("joined_at", Required = Required.DisallowNull)]
        public DateTime JoinedAt { get; set; }

        [JsonProperty("title", Required = Required.DisallowNull)]
        public string Title { get; set; }

        [JsonProperty("hasPassword", Required = Required.DisallowNull)]
        public bool HasPassword { get; set; }

        [JsonProperty("authToken", Required = Required.DisallowNull)]
        public string AuthToken { get; set; }

        [JsonProperty("authentication_token", Required = Required.DisallowNull)]
        public string AuthenticationToken { get; set; }

        [JsonProperty("forumId", Required = Required.DisallowNull)]
        public object ForumId { get; set; }

        [JsonProperty("plexServers", Required = Required.Always)]
        public List<PlexServerDTO> PlexServers { get; set; }
    }
}
