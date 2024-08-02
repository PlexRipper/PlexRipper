using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Converters;

public class StringToBool : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringBool = reader.GetString();
            switch (stringBool)
            {
                case "0":
                    return false;
                case "1":
                    return true;
                default:
                    return false;
            }
        }

        return false;
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
