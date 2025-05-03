using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexMovieMediaDataConfiguration : IEntityTypeConfiguration<PlexMovieMediaData>
{
    public void Configure(EntityTypeBuilder<PlexMovieMediaData> builder)
    {
        builder.UseTpcMappingStrategy();

        builder
            .HasMany(x => x.Parts)
            .WithOne(x => x.PlexMovieMediaData)
            .HasForeignKey(x => x.PlexMovieMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexMovie)
            .WithMany(x => x.MediaDataList)
            .HasForeignKey(x => x.PlexMovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
