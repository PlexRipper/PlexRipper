using AutoMapper;
using System;

namespace PlexRipper.Domain.AutoMapper.ValueConverters
{
    public class StringToDateTimeUTC : IValueConverter<string, DateTime>
    {
        public DateTime Convert(string sourceMember, ResolutionContext context)
        {
            if (DateTime.TryParse(sourceMember, out DateTime dateTimeResult))
            {
                return dateTimeResult;
            }

            return DateTime.MinValue;
        }
    }
}
