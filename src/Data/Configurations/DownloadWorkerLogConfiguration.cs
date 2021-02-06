using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlexRipper.Domain;
using Serilog.Events;

namespace PlexRipper.Data.Configurations
{
    public class DownloadWorkerLogConfiguration : IEntityTypeConfiguration<DownloadWorkerLog>
    {
        public void Configure(EntityTypeBuilder<DownloadWorkerLog> builder)
        {
            builder
                .Property(e => e.LogLevel)
                .HasConversion(new EnumToStringConverter<LogEventLevel>());
        }
    }
}