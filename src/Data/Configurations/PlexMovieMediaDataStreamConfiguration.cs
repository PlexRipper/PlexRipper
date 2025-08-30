using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexMovieMediaDataStreamConfiguration : IEntityTypeConfiguration<PlexMovieMediaDataStream>
{
    public void Configure(EntityTypeBuilder<PlexMovieMediaDataStream> builder)
    {
        builder.HasOne(x => x.PlexMovie).WithMany().HasForeignKey(x => x.PlexMovieId).OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexMovieMediaData)
            .WithMany()
            .HasForeignKey(x => x.PlexMovieMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexMovieMediaDataPart)
            .WithMany(x => x.Streams)
            .HasForeignKey(x => x.PlexMovieMediaDataPartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
