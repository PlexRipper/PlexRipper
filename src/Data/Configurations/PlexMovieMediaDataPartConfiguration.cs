using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexMovieMediaDataPartConfiguration : IEntityTypeConfiguration<PlexMovieMediaDataPart>
{
    public void Configure(EntityTypeBuilder<PlexMovieMediaDataPart> builder)
    {
        builder
            .HasMany(x => x.Streams)
            .WithOne(x => x.PlexMovieMediaDataPart)
            .HasForeignKey(x => x.PlexMovieMediaDataPartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PlexMovie).WithMany().HasForeignKey(x => x.PlexMovieId).OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexMovieMediaData)
            .WithMany(x => x.Parts)
            .HasForeignKey(x => x.PlexMovieMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
