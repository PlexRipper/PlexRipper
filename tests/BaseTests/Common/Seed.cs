namespace PlexRipper.BaseTests;

public class Seed
{
    public int Value { get; private set; }

    public Seed(int seed = 0)
    {
        Value = seed;
    }

    public int Next() => Value++;
}
