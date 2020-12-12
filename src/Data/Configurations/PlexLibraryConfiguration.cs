using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
    public class PlexLibraryConfiguration : IEntityTypeConfiguration<PlexLibrary>
    {
        public void Configure(EntityTypeBuilder<PlexLibrary> builder)
        {
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

            var converter = new EnumToStringConverter<PlexMediaType>();
            builder
                .Property(e => e.Type)
                .HasConversion(converter);
        }
    }
}