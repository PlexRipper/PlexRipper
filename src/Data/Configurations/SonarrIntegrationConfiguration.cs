namespace Reaparr.Data.Configurations;

public class SonarrIntegrationConfiguration : IEntityTypeConfiguration<SonarrIntegration>
{
    public void Configure(EntityTypeBuilder<SonarrIntegration> builder)
    {
        builder
            .Property(x => x.ProvisioningState)
            .HasMaxLength(20)
            .HasConversion(x => x.ToIntegrationProvisioningStateString(), x => x.ToIntegrationProvisioningState())
            .IsUnicode(false);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.BaseUrl).IsUnique();
        builder.HasIndex(x => x.Category).IsUnique();
        builder.HasIndex(x => x.ReaparrApiKey).IsUnique();
        builder
            .HasOne(x => x.DownloadFolder)
            .WithMany()
            .HasForeignKey(x => x.DownloadFolderId)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasMany(x => x.DownloadTasks)
            .WithOne(x => x.SonarrIntegration)
            .HasForeignKey(x => x.SonarrIntegrationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
