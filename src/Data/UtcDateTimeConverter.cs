using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Reaparr.Data;

public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => ToUtc(value),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        )
    {
    }

    private static DateTime ToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value,
        };
}
