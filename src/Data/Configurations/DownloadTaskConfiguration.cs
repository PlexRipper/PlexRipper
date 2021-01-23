using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
    public class DownloadTaskConfiguration : IEntityTypeConfiguration<DownloadTask>
    {
        public void Configure(EntityTypeBuilder<DownloadTask> builder)
        {
            builder
                .HasMany(x => x.DownloadWorkerTasks)
                .WithOne(x => x.DownloadTask)
                .HasForeignKey(x => x.DownloadTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Property(e => e.MediaType)
                .HasConversion(new EnumToStringConverter<PlexMediaType>());

            builder
                .Property(e => e.DownloadStatus)
                .HasConversion(new EnumToStringConverter<DownloadStatus>());

            builder
                .Property(x => x.MetaData)
                .HasJsonValueConversion();

        }
    }
}