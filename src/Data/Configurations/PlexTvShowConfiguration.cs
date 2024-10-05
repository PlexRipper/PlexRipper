using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowConfiguration : IEntityTypeConfiguration<PlexTvShow>
{
    public void Configure(EntityTypeBuilder<PlexTvShow> builder)
    {
        // NOTE: This has been added to PlexRipperDbContext.OnModelCreating
        // Based on: https://stackoverflow.com/a/63992731/8205497
        // builder
        //     .Property(x => x.MediaData)
        //     .HasJsonValueConversion();

        builder.Property(c => c.SortTitle).UseCollation(OrderByNaturalExtensions.CollationName);
    }
}
