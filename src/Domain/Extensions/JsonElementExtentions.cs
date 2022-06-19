using System;
using System.Text.Json;

namespace PlexRipper.Domain
{
    public static class JsonElementExtentions
    {
        public static object GetTypedValue(this JsonElement jsonElement, Type type)
        {
            switch (type)
            {
                case { } t when t == typeof(int):
                    return jsonElement.GetInt32();
                case { } t when t == typeof(bool):
                    return jsonElement.GetBoolean();
                case { } t when t == typeof(string):
                    return jsonElement.GetString();
                case { } t when t == typeof(ViewMode):
                    return jsonElement.GetString().ToViewMode();
                default:
                    throw new ArgumentException($"Typename {type.FullName} of {type} is not supported when parsing");
            }
        }
    }
}