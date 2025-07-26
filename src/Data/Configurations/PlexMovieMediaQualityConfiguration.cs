using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexMovieMediaQualityConfiguration : IEntityTypeConfiguration<PlexMovieMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexMovieMediaQuality> builder)
    {
        builder.HasKey(bc => new
        {
            bc.PlexMediaQualityId,
            bc.PlexLibraryId,
            bc.PlexMovieId,
        });
    }
}
