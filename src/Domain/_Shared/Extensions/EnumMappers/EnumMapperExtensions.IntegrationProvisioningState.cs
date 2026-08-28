namespace Reaparr.Domain;

public static partial class EnumMapperExtensions
{
    public static IntegrationProvisioningState ToIntegrationProvisioningState(this string value) =>
        Enum.Parse<IntegrationProvisioningState>(value, ignoreCase: false);

    public static string ToIntegrationProvisioningStateString(this IntegrationProvisioningState value) =>
        value.ToString();
}
