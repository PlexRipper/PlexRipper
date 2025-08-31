using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexActorConfiguration : IEntityTypeConfiguration<PlexActor>
{
    public void Configure(EntityTypeBuilder<PlexActor> builder)
    {
        builder.HasIndex(x => x.Key).IsUnique();
    }
}
