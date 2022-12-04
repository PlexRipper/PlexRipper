using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexServerConnectionConfiguration : IEntityTypeConfiguration<PlexServerConnection>
{
    public void Configure(EntityTypeBuilder<PlexServerConnection> builder) { }
}