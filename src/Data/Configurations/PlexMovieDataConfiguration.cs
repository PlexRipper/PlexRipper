using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
    public class PlexMovieDataConfiguration : IEntityTypeConfiguration<PlexMovieData>
    {
        public void Configure(EntityTypeBuilder<PlexMovieData> builder)
        {
            // builder.HasMany(x => x.Parts)
            //     .WithOne(x => x.PlexMovieData)
            //     .HasForeignKey(x => x.PlexMovieDataId);
            // builder.HasOne(x => x.Part1)
            //     .WithOne()
            //     .HasForeignKey(typeof(PlexMovieData), nameof(PlexMovieData.Part1Id));
            // builder.HasOne(x => x.Part2)
            //     .WithOne()
            //     .HasForeignKey(typeof(PlexMovieData), nameof(PlexMovieData.Part2Id));
            // builder.HasOne(x => x.Part3)
            //     .WithOne()
            //     .HasForeignKey(typeof(PlexMovieData), nameof(PlexMovieData.Part3Id));
            // builder.HasOne(x => x.Part4)
            //     .WithOne()
            //     .HasForeignKey(typeof(PlexMovieData), nameof(PlexMovieData.Part4Id));
            // builder.HasOne(x => x.Part5)
            //     .WithOne()
            //     .HasForeignKey(typeof(PlexMovieData), nameof(PlexMovieData.Part5Id));
            // builder.HasOne(x => x.Part6)
            //     .WithOne()
            //     .HasForeignKey(typeof(PlexMovieData), nameof(PlexMovieData.Part6Id));
            // builder.HasOne(x => x.Part7)
            //     .WithOne()
            //     .HasForeignKey(typeof(PlexMovieData), nameof(PlexMovieData.Part7Id));
            // builder.HasOne(x => x.Part8)
            //     .WithOne()
            //     .HasForeignKey(typeof(PlexMovieData), nameof(PlexMovieData.Part8Id));
        }
    }
}