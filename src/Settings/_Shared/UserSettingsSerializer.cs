using System.Text.Json;
using Reaparr.Settings.Contracts;

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
    ///  If the deserialization fails, a new instance of the <see cref="UserSettings"/> is returned.
    /// </summary>
    /// <param name="json"> The JSON string to deserialize. </param>
    /// <returns> The deserialized <see cref="UserSettings"/>. </returns>
    public static UserSettings Deserialize(string json)
    {
        try
        {
            if (json == string.Empty)
                return new UserSettings();

            return JsonSerializer.Deserialize<UserSettings>(json, DefaultJsonSerializerOptions.UserSettingsOptions)
                ?? new UserSettings();
        }
        catch (Exception e)
        {
            _log.Here().Error("Failed to deserialize settings");
            _log.Here().ErrorResult(e);
        }

        return new UserSettings();
    }
}
