using System.Text.Json;
using Logging.Interface;
using PlexRipper.Domain.Config;
using Settings.Contracts;

namespace PlexRipper.Settings;

public static class UserSettingsSerializer
{
    private static readonly ILog Log = LogManager.CreateLogInstance(typeof(ResultExtensions));

    public static string Serialize(IUserSettings userSettings)
    {
        try
        {
            return JsonSerializer.Serialize(userSettings, DefaultJsonSerializerOptions.UserSettingsOptions);
        }
        catch (Exception e)
        {
            Log.ErrorLine("Failed to serialize settings");
            Log.Error(e);
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
            Log.ErrorLine("Failed to deserialize settings");
            Log.Error(e);
        }

        return new UserSettings();
    }
}
