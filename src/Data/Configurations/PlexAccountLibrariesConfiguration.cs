using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexAccountLibrariesConfiguration : IEntityTypeConfiguration<PlexAccountLibrary>
{
    public void Configure(EntityTypeBuilder<PlexAccountLibrary> builder)
    {
        builder.HasKey(bc => new
        {
            bc.PlexAccountId,
            bc.PlexLibraryId,
            bc.PlexServerId,
        });

        builder.HasOne(x => x.PlexLibrary);
        builder.HasOne(x => x.PlexServer);
        builder.HasOne(x => x.PlexAccount);
    }
}
