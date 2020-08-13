using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data.Configurations
{
    public class DownloadTaskConfiguration : IEntityTypeConfiguration<DownloadTask>
    {
        public void Configure(EntityTypeBuilder<DownloadTask> builder)
        {

        }
    }
}
