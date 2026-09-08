namespace Reaparr.Domain;

public static partial class EnumMapperExtensions
{
    public static IntegrationType ToIntegrationType(this string value) =>
        Enum.Parse<IntegrationType>(value, ignoreCase: false);

    public static string ToIntegrationTypeString(this IntegrationType value) => value.ToString();
}
