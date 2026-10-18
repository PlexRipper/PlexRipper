namespace Reaparr.Domain;

public class StreamLanguage : BaseEntity
{
    public required string LanguageCode { get; init; }

    public required string LanguageTag { get; init; }
}
