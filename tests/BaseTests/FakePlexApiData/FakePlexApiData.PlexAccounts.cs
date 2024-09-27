using Bogus;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexRipper.PlexApi;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    public static Faker<PostUsersSignInDataUserPlexAccount> GetPlexSignInResponse(
        Action<UnitTestDataConfig>? options = null
    )
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<PostUsersSignInDataUserPlexAccount>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Id, f => f.Random.Number(99999999))
            .RuleFor(x => x.Uuid, f => f.Random.Guid().ToString())
            .RuleFor(x => x.Username, f => f.Internet.UserName())
            .RuleFor(x => x.Title, f => f.Internet.UserName())
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.FriendlyName, _ => "")
            .RuleFor(x => x.Locale, _ => "EN")
            .RuleFor(x => x.Confirmed, f => f.Random.Bool())
            .RuleFor(x => x.JoinedAt, f => f.Date.Past(3).Ticks)
            .RuleFor(x => x.EmailOnlyAuth, f => f.Random.Bool())
            .RuleFor(x => x.HasPassword, _ => true)
            .RuleFor(x => x.Protected, f => f.Random.Bool())
            .RuleFor(x => x.Thumb, f => f.Internet.UrlWithPath())
            .RuleFor(x => x.AuthToken, f => f.Random.Guid().ToString())
            .RuleFor(x => x.MailingListStatus, _ => PostUsersSignInDataMailingListStatus.Active)
            .RuleFor(x => x.MailingListActive, f => f.Random.Bool())
            .RuleFor(x => x.ScrobbleTypes, _ => "")
            .RuleFor(x => x.Country, _ => "EN")
            .RuleFor(x => x.Subscription, _ => null)
            .RuleFor(x => x.SubscriptionDescription, _ => null)
            .RuleFor(x => x.Restricted, f => f.Random.Bool())
            .RuleFor(x => x.Anonymous, _ => null)
            .RuleFor(x => x.Home, f => f.Random.Bool())
            .RuleFor(x => x.Guest, f => f.Random.Bool())
            .RuleFor(x => x.HomeSize, f => f.Random.Number(20))
            .RuleFor(x => x.HomeAdmin, f => f.Random.Bool())
            .RuleFor(x => x.MaxHomeSize, f => f.Random.Number(20))
            .RuleFor(x => x.RememberExpiresAt, f => f.Date.Future(1).Ticks)
            .RuleFor(x => x.Profile, _ => null)
            .RuleFor(x => x.Entitlements, _ => null)
            .RuleFor(x => x.Subscriptions, _ => null)
            .RuleFor(x => x.PastSubscriptions, _ => null)
            .RuleFor(x => x.Trials, _ => null)
            .RuleFor(x => x.Services, _ => null)
            .RuleFor(x => x.AdsConsent, _ => null)
            .RuleFor(x => x.AdsConsentSetAt, _ => null)
            .RuleFor(x => x.AdsConsentReminderAt, _ => null)
            .RuleFor(x => x.Pin, _ => null)
            .RuleFor(x => x.AttributionPartner, _ => null)
            .RuleFor(x => x.Roles, _ => [""])
            .RuleFor(x => x.ExperimentalFeatures, f => f.Random.Bool())
            .RuleFor(x => x.TwoFactorEnabled, f => f.Random.Bool())
            .RuleFor(x => x.BackupCodesCreated, f => f.Random.Bool());
    }

    public static PlexErrorsResponseDTO GetFailedPlexSignInResponse() =>
        new()
        {
            Errors =
            [
                new PlexErrorDTO
                {
                    Code = 1001,
                    Message = "User could not be authenticated",
                    Status = 401,
                },
            ],
        };
}
