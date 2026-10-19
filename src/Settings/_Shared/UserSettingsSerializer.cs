using System.Text.Json;

namespace Reaparr.Settings;

public static class UserSettingsSerializer
{
    private static readonly ILogger _log = LogFactory.Create(typeof(UserSettingsSerializer));

    public static string Serialize(IUserSettings userSettings)
    {
        try
        {
            return JsonSerializer.Serialize(userSettings, DefaultJsonSerializerOptions.UserSettingsOptions);
        }
        catch (Exception e)
        {
            _log.Here().Error("Failed to serialize settings");
            _log.Here().ErrorResult(e);
        }

        return string.Empty;
    }

    /// <summary>
    ///  Deserialize the user settings from a JSON string.
    ///  Parse errors propagate to the caller.
    /// </summary>
    /// <param name="json"> The JSON string to deserialize. </param>
    /// <returns> The deserialized <see cref="UserSettings"/>. </returns>
    public static UserSettings Deserialize(string json)
    {
        if (json == string.Empty)
            return new UserSettings();

        return JsonSerializer.Deserialize<UserSettings>(json, DefaultJsonSerializerOptions.UserSettingsOptions)
            ?? new UserSettings();
    }
}
