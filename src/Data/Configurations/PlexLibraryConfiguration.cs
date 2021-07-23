using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
    public class PlexLibraryConfiguration : IEntityTypeConfiguration<PlexLibrary>
    {
        public void Configure(EntityTypeBuilder<PlexLibrary> builder)
        {
            builder
                .HasOne(x => x.PlexServer)
                .WithMany(x => x.PlexLibraries)
                .HasForeignKey(x => x.PlexServerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(x => x.Movies)
                .WithOne(x => x.PlexLibrary)
                .HasForeignKey(x => x.PlexLibraryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(x => x.TvShows)
                .WithOne(x => x.PlexLibrary)
                .HasForeignKey(x => x.PlexLibraryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(x => x.PlexAccountLibraries)
                .WithOne(x => x.PlexLibrary)
                .HasForeignKey(x => x.PlexLibraryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(x => x.DownloadTasks)
                .WithOne(x => x.PlexLibrary)
                .HasForeignKey(x => x.PlexLibraryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.DefaultDestination)
                .WithMany(x => x.PlexLibraries)
                .HasForeignKey(x => x.DefaultDestinationId)
                .OnDelete(DeleteBehavior.SetNull);


            var converter = new EnumToStringConverter<PlexMediaType>();
            builder
                .Property(e => e.Type)
                .HasConversion(converter);

            builder
                .Property(x => x.MetaData)
                .HasJsonValueConversion();
        }
    }
}