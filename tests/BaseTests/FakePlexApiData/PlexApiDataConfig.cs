namespace PlexRipper.BaseTests;

public class PlexApiDataConfig : BaseConfig<PlexApiDataConfig>
{
    public int GetSeed() => ++Seed;

    public int LibraryCount { get; set; } = 3;

    public int LibraryMetaDataCount { get; set; } = 50;
}