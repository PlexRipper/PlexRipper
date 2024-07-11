using AutoMapper;

namespace PlexRipper.Domain.AutoMapper.ValueConverters;

public class UnixLongStringToDateTimeUTC : IValueConverter<string, DateTime>
{
    public DateTime Convert(string sourceMember, ResolutionContext context)
    {
        // Safely try to convert to long first, it can happen that huge numbers might be returned from the DTO's
        long l1;
        if (!string.IsNullOrEmpty(sourceMember) && long.TryParse(sourceMember, out l1))
            return DateTimeOffset.FromUnixTimeSeconds(l1).UtcDateTime;

        return DateTime.MinValue;
    }
}
