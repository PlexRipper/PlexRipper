using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexMovieMediaDataPartConfiguration : IEntityTypeConfiguration<PlexMovieMediaDataPart>
{
    public void Configure(EntityTypeBuilder<PlexMovieMediaDataPart> builder)
    {
        builder.UseTpcMappingStrategy();

        // Configure one-to-many relationship with Streams
        builder
            .HasMany(x => x.Streams)
            .WithOne(x => x.PlexMovieMediaDataPart)
            .HasForeignKey(x => x.PlexMovieMediaDataPartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with PlexMovie
        builder.HasOne(x => x.PlexMovie).WithMany().HasForeignKey(x => x.PlexMovieId).OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with PlexMovieMediaData
        builder
            .HasOne(x => x.PlexMovieMediaData)
            .WithMany(x => x.Parts)
            .HasForeignKey(x => x.PlexMovieMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
