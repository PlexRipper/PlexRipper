using System.Reflection;
using System.Runtime.Serialization;

namespace Reaparr.Domain;

public static class EnumExtensions
{
    public static string ToEnumMemberValue(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();

        return member?.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? value.ToString();
    }
}
