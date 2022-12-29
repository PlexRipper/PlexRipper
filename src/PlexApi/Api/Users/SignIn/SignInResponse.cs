using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Api.Users.SignIn;

public class SignInResponse
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
    public object Locale { get; set; }

    [JsonPropertyName("confirmed")]
    public bool Confirmed { get; set; }

    [JsonPropertyName("joinedAt")]
    public long JoinedAt { get; set; }

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
    public SignIn_Subscription Subscription { get; set; }

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

    [JsonPropertyName("rememberExpiresAt")]
    public long RememberExpiresAt { get; set; }

    [JsonPropertyName("profile")]
    public SignIn_Profile Profile { get; set; }

    [JsonPropertyName("entitlements")]
    public List<object> Entitlements { get; set; }

    [JsonPropertyName("subscriptions")]
    public List<object> Subscriptions { get; set; }

    [JsonPropertyName("pastSubscriptions")]
    public List<SignIn_PastSubscription> PastSubscriptions { get; set; }

    [JsonPropertyName("trials")]
    public List<object> Trials { get; set; }

    [JsonPropertyName("services")]
    public List<SignIn_Service> Services { get; set; }

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

public class SignIn_Billing
{
    [JsonPropertyName("paymentMethodId")]
    public object PaymentMethodId { get; set; }

    [JsonPropertyName("internalPaymentMethod")]
    public SignIn_InternalPaymentMethod InternalPaymentMethod { get; set; }
}

public class SignIn_InternalPaymentMethod { }

public class SignIn_PastSubscription
{
    [JsonPropertyName("id")]
    public object Id { get; set; }

    [JsonPropertyName("mode")]
    public object Mode { get; set; }

    [JsonPropertyName("renewsAt")]
    public object RenewsAt { get; set; }

    [JsonPropertyName("endsAt")]
    public int EndsAt { get; set; }

    [JsonPropertyName("billing")]
    public SignIn_Billing Billing { get; set; }

    [JsonPropertyName("canceled")]
    public bool Canceled { get; set; }

    [JsonPropertyName("gracePeriod")]
    public bool GracePeriod { get; set; }

    [JsonPropertyName("onHold")]
    public bool OnHold { get; set; }

    [JsonPropertyName("canReactivate")]
    public bool CanReactivate { get; set; }

    [JsonPropertyName("canUpgrade")]
    public bool CanUpgrade { get; set; }

    [JsonPropertyName("canDowngrade")]
    public bool CanDowngrade { get; set; }

    [JsonPropertyName("canConvert")]
    public bool CanConvert { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("transfer")]
    public object Transfer { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }
}

public class SignIn_Profile
{
    [JsonPropertyName("autoSelectAudio")]
    public bool AutoSelectAudio { get; set; }

    [JsonPropertyName("defaultAudioLanguage")]
    public string DefaultAudioLanguage { get; set; }

    [JsonPropertyName("defaultSubtitleLanguage")]
    public string DefaultSubtitleLanguage { get; set; }

    [JsonPropertyName("autoSelectSubtitle")]
    public int AutoSelectSubtitle { get; set; }

    [JsonPropertyName("defaultSubtitleAccessibility")]
    public int DefaultSubtitleAccessibility { get; set; }

    [JsonPropertyName("defaultSubtitleForced")]
    public int DefaultSubtitleForced { get; set; }
}

public class SignIn_Service
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("secret")]
    public string Secret { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }
}

public class SignIn_Subscription
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("subscribedAt")]
    public DateTime SubscribedAt { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("paymentService")]
    public object PaymentService { get; set; }

    [JsonPropertyName("plan")]
    public object Plan { get; set; }

    [JsonPropertyName("features")]
    public List<string> Features { get; set; }
}