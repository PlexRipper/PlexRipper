using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models
{
    public class PlexAccountDTO
    {
        [JsonConverter(typeof(IntValueConverter))]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("confirmed")]
        public bool Confirmed { get; set; }

        [JsonPropertyName("emailOnlyAuth")]
        public bool EmailOnlyAuth { get; set; }

        [JsonPropertyName("hasPassword")]
        public bool HasPassword { get; set; }

        [JsonPropertyName("protected")]
        public bool Protected { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }

        [JsonPropertyName("authToken")]
        public string AuthToken { get; set; }

        [JsonPropertyName("mailingListStatus")]
        public string MailingListStatus { get; set; }

        [JsonPropertyName("mailingListActive")]
        public bool MailingListActive { get; set; }

        [JsonPropertyName("scrobbleTypes")]
        public string ScrobbleTypes { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("subscription")]
        public Subscription Subscription { get; set; }

        [JsonPropertyName("subscriptionDescription")]
        public object SubscriptionDescription { get; set; }

        [JsonPropertyName("restricted")]
        public bool Restricted { get; set; }

        [JsonPropertyName("anonymous")]
        public object Anonymous { get; set; }

        [JsonPropertyName("home")]
        public bool Home { get; set; }

        [JsonPropertyName("guest")]
        public bool Guest { get; set; }

        [JsonPropertyName("homeSize")]
        public int HomeSize { get; set; }

        [JsonPropertyName("homeAdmin")]
        public bool HomeAdmin { get; set; }

        [JsonPropertyName("maxHomeSize")]
        public int MaxHomeSize { get; set; }

        [JsonPropertyName("certificateVersion")]
        public int CertificateVersion { get; set; }

        [JsonPropertyName("rememberExpiresAt")]
        public int RememberExpiresAt { get; set; }

        [JsonPropertyName("trials")]
        public List<object> Trials { get; set; }

        [JsonPropertyName("adsConsent")]
        public object AdsConsent { get; set; }

        [JsonPropertyName("adsConsentSetAt")]
        public object AdsConsentSetAt { get; set; }

        [JsonPropertyName("adsConsentReminderAt")]
        public object AdsConsentReminderAt { get; set; }

        [JsonPropertyName("experimentalFeatures")]
        public bool ExperimentalFeatures { get; set; }

        [JsonPropertyName("twoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }

        [JsonPropertyName("backupCodesCreated")]
        public bool BackupCodesCreated { get; set; }
    }
}