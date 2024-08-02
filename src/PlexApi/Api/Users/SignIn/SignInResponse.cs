using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Api.Users.SignIn;

public class SignInResponse
{
    [JsonConverter(typeof(IntValueConverter))]
    public int Id { get; set; }

    public string Uuid { get; set; }

    public string Username { get; set; }

    public string Title { get; set; }

    public string Email { get; set; }

    public string FriendlyName { get; set; }

    public object Locale { get; set; }

    public bool Confirmed { get; set; }

    public long JoinedAt { get; set; }

    public bool EmailOnlyAuth { get; set; }

    public bool HasPassword { get; set; }

    public bool Protected { get; set; }

    public string Thumb { get; set; }

    public string AuthToken { get; set; }

    public string MailingListStatus { get; set; }

    public bool MailingListActive { get; set; }

    public string ScrobbleTypes { get; set; }

    public string Country { get; set; }

    public SignIn_Subscription Subscription { get; set; }

    public object SubscriptionDescription { get; set; }

    public bool Restricted { get; set; }

    public object Anonymous { get; set; }

    public bool Home { get; set; }

    public bool Guest { get; set; }

    public int HomeSize { get; set; }

    public bool HomeAdmin { get; set; }

    public int MaxHomeSize { get; set; }

    public long RememberExpiresAt { get; set; }

    public SignIn_Profile Profile { get; set; }

    public List<object> Entitlements { get; set; }

    public List<object> Subscriptions { get; set; }

    public List<SignIn_PastSubscription> PastSubscriptions { get; set; }

    public List<object> Trials { get; set; }

    public List<SignIn_Service> Services { get; set; }

    public object AdsConsent { get; set; }

    public object AdsConsentSetAt { get; set; }

    public object AdsConsentReminderAt { get; set; }

    public bool ExperimentalFeatures { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public bool BackupCodesCreated { get; set; }
}

public class SignIn_Billing
{
    public object PaymentMethodId { get; set; }

    public SignIn_InternalPaymentMethod InternalPaymentMethod { get; set; }
}

public class SignIn_InternalPaymentMethod { }

public class SignIn_PastSubscription
{
    public object Id { get; set; }

    public object Mode { get; set; }

    public object RenewsAt { get; set; }

    public int EndsAt { get; set; }

    public SignIn_Billing Billing { get; set; }

    public bool Canceled { get; set; }

    public bool GracePeriod { get; set; }

    public bool OnHold { get; set; }

    public bool CanReactivate { get; set; }

    public bool CanUpgrade { get; set; }

    public bool CanDowngrade { get; set; }

    public bool CanConvert { get; set; }

    public string Type { get; set; }

    public object Transfer { get; set; }

    public string State { get; set; }
}

public class SignIn_Profile
{
    public bool AutoSelectAudio { get; set; }

    public string DefaultAudioLanguage { get; set; }

    public string DefaultSubtitleLanguage { get; set; }

    public int AutoSelectSubtitle { get; set; }

    public int DefaultSubtitleAccessibility { get; set; }

    public int DefaultSubtitleForced { get; set; }
}

public class SignIn_Service
{
    public string Identifier { get; set; }

    public string Endpoint { get; set; }

    public string Token { get; set; }

    public string Secret { get; set; }

    public string Status { get; set; }
}

public class SignIn_Subscription
{
    public bool Active { get; set; }

    public DateTime SubscribedAt { get; set; }

    public string Status { get; set; }

    public object PaymentService { get; set; }

    public object Plan { get; set; }

    public List<string> Features { get; set; }
}
