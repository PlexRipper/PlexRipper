using PlexRipper.Application.Common.Interfaces;
using System;

namespace PlexRipper.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;
    }
}
