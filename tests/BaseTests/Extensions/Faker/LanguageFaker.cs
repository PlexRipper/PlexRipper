using System.Globalization;
using System.Globalization;
using Bogus.Premium;

namespace PlexRipper.BaseTests;

public static class LanguageFaker
{
    /// <summary>
    /// Language names (English names like "English", "French")
    /// </summary>
    public static readonly HashSet<string> LanguageNames =
    [
        .. CultureInfo
            .GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures)
            .Select(c => c.EnglishName)
            .Where(name => !string.IsNullOrWhiteSpace(name)),
    ];

    /// <summary>
    /// Language tags (e.g., "en-US", "fr-FR")
    /// </summary>
    public static readonly HashSet<string> LanguageTags =
    [
        .. CultureInfo
            .GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures)
            .Select(c => c.Name)
            .Where(tag => !string.IsNullOrWhiteSpace(tag)),
    ];

    //
    /// <summary>
    /// ISO language codes (e.g., "en", "fr")
    /// </summary>
    public static readonly HashSet<string> LanguageCodes =
    [
        .. CultureInfo
            .GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures)
            .Select(c => c.TwoLetterISOLanguageName)
            .Where(code => !string.IsNullOrWhiteSpace(code)),
    ];

    public static LanguageDataSet Language(this Faker faker)
    {
        return ContextHelper.GetOrSet(faker, () => new LanguageDataSet(faker));
    }
}

public class LanguageDataSet : DataSet
{
    private readonly Faker _faker;

    public LanguageDataSet(Faker faker)
    {
        _faker = faker;
    }

    public string LanguageName() => _faker.PickRandomFromDataset(LanguageFaker.LanguageNames);

    public string LanguageTag() => _faker.PickRandomFromDataset(LanguageFaker.LanguageTags);

    public string LanguageCode() => _faker.PickRandomFromDataset(LanguageFaker.LanguageCodes);
}
