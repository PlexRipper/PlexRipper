namespace Reaparr.Data.Configurations;

public class SonarrIntegrationConfiguration : IEntityTypeConfiguration<SonarrIntegration>
{
    public void Configure(EntityTypeBuilder<SonarrIntegration> builder)
    {
        builder.ToTable("IntegrationsSonarr");
        builder
            .Property(x => x.ProvisioningState)
            .HasMaxLength(20)
            .HasConversion(x => x.ToIntegrationProvisioningStateString(), x => x.ToIntegrationProvisioningState())
            .IsUnicode(false);
        builder.HasIndex(x => x.DisplayName).IsUnique();
        builder.HasIndex(x => x.BaseUrl).IsUnique();
        builder.HasIndex(x => x.Category).IsUnique();
        builder.Property(x => x.QBittorrentApiKey).HasMaxLength(32).IsUnicode(false);
        builder.Property(x => x.TorznabApiKey).HasMaxLength(32).IsUnicode(false);
        builder
            .Property(x => x.LastConnectionTestStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TestConnectionStatus.Unknown)
            .IsUnicode(false);
        builder.Property(x => x.LastConnectionTestErrorMessage).HasMaxLength(200);
        builder
            .HasOne(x => x.DownloadFolder)
            .WithMany()
            .HasForeignKey(x => x.DownloadFolderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasMany(x => x.DownloadTasks)
            .WithOne(x => x.SonarrIntegration)
            .HasForeignKey(x => x.SonarrIntegrationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
