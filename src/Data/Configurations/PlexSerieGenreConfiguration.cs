using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities.JoinTables;

namespace PlexRipper.Data.Configurations
{
    public class PlexSerieGenreConfiguration : IEntityTypeConfiguration<PlexSerieGenre>
    {
        public void Configure(EntityTypeBuilder<PlexSerieGenre> builder)
        {
            builder
                .HasKey(bc => new { bc.PlexSerieId, bc.PlexGenreId });
        }
    }
}
