namespace PlexRipper.BaseTests;

public class Seed
{
    public int Value { get; private set; }

    public Seed(int seed)
    {
        Value = seed;
    }

    public int Next() => Value++;
}
