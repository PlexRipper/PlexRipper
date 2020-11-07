using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexAccountDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty("username", Required = Required.Always)]
        public string Username { get; set; }

        [JsonProperty("password", Required = Required.Always)]
        public string Password { get; set; }

        [JsonProperty("isEnabled", Required = Required.Always)]
        public bool IsEnabled { get; set; }

        [JsonProperty("isMain", Required = Required.Always)]
        public bool IsMain { get; set; }

        [JsonProperty("isValidated", Required = Required.Always)]
        public bool IsValidated { get; set; }

        [JsonProperty("validatedAt", Required = Required.Always)]
        public DateTime ValidatedAt { get; set; }

        [JsonProperty("uuid", Required = Required.Always)]
        public string Uuid { get; set; }

        [JsonProperty("email", Required = Required.AllowNull)]
        public string Email { get; set; }

        [JsonProperty("joined_at", Required = Required.Always)]
        public DateTime JoinedAt { get; set; }

        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty("hasPassword", Required = Required.Always)]
        public bool HasPassword { get; set; }

        [JsonProperty("authToken", Required = Required.Always)]
        public string AuthToken { get; set; }

        [JsonProperty("forumId", Required = Required.Always)]
        public int ForumId { get; set; }

        [JsonProperty("plexServers", Required = Required.Always)]
        public List<PlexServerDTO> PlexServers { get; set; }
    }
}