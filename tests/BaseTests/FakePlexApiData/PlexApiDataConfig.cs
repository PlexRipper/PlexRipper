namespace PlexRipper.BaseTests;

public class PlexApiDataConfig : BaseConfig<PlexApiDataConfig>
{
    public int GetSeed() => ++Seed;

    public int LibraryCount { get; init; } = 3;

    public int LibraryMetaDataCount { get; init; } = 50;
}
