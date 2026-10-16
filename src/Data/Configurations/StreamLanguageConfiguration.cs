namespace Reaparr.Data;

public class StreamLanguageConfiguration : IEntityTypeConfiguration<StreamLanguage>
{
    public void Configure(EntityTypeBuilder<StreamLanguage> builder)
    {
        builder.Property(x => x.LanguageCode).IsRequired();
        builder.Property(x => x.LanguageTag).IsRequired();
        builder.HasIndex(x => new { x.LanguageCode, x.LanguageTag }).IsUnique();
    }
}
