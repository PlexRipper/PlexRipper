using System.Text.Json;
using Logging.Interface;
using PlexRipper.Domain.Config;
using Settings.Contracts;

namespace PlexRipper.Settings;

public static class UserSettingsSerializer
{
    private static ILog _log = LogManager.CreateLogInstance(typeof(ResultExtensions));

    public static string Serialize(SettingsModel settingsModel)
    {
        try
        {
            return JsonSerializer.Serialize(settingsModel, DefaultJsonSerializerOptions.ConfigManagerOptions);
        }
        catch (Exception e)
        {
            _log.ErrorLine("Failed to serialize settings");
            _log.Error(e);
        }

        return string.Empty;
    }

    /// <summary>
    ///  Deserialize the user settings from a JSON string.
    ///  If the deserialization fails, a new instance of the <see cref="SettingsModel"/> is returned.
    /// </summary>
    /// <param name="json"> The JSON string to deserialize. </param>
    /// <returns> The deserialized <see cref="SettingsModel"/>. </returns>
    public static SettingsModel Deserialize(string json)
    {
        try
        {
            if (json == string.Empty)
                return new SettingsModel();

            return JsonSerializer.Deserialize<SettingsModel>(json, DefaultJsonSerializerOptions.ConfigManagerOptions)
                ?? new SettingsModel();
        }
        catch (Exception e)
        {
            _log.ErrorLine("Failed to deserialize settings");
            _log.Error(e);
        }

        return new SettingsModel();
    }
}
