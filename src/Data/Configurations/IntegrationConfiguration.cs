namespace Reaparr.Data.Configurations;

public class IntegrationConfiguration : IEntityTypeConfiguration<Integration>
{
    public void Configure(EntityTypeBuilder<Integration> builder)
    {
        builder
            .Property(x => x.Type)
            .HasMaxLength(10)
            .HasConversion(x => x.ToIntegrationTypeString(), x => x.ToIntegrationType())
            .IsUnicode(false);

        builder
            .Property(x => x.ProvisioningState)
            .HasMaxLength(20)
            .HasConversion(x => x.ToIntegrationProvisioningStateString(), x => x.ToIntegrationProvisioningState())
            .IsUnicode(false);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => new { x.Type, x.BaseUrl }).IsUnique();
        builder.HasIndex(x => x.Category).IsUnique();
        builder.HasIndex(x => x.ReaparrApiKey).IsUnique();

        builder
            .HasMany(x => x.DownloadTasks)
            .WithOne(x => x.Integration)
            .HasForeignKey(x => x.IntegrationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
