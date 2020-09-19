using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
    public class PlexMovieConfiguration : IEntityTypeConfiguration<PlexMovie>
    {
        public void Configure(EntityTypeBuilder<PlexMovie> builder)
        {
            builder.HasMany(x => x.PlexMovieDatas)
                .WithOne(x => x.PlexMovie)
                .HasForeignKey(x => x.PlexMovieId);
        }
    }
}