namespace Reaparr.BaseTests;

public class Seed
{
    public int Value { get; private set; }

    public Seed(int seed)
    {
        seed.ShouldBeGreaterThan(0);
        Value = seed;
    }

    public int Next() => Value++;

    public static ICollection<Seed> Generate(int count)
    {
        if (count <= 0)
            return new List<Seed>();

        var rnd = new Random();
        return Enumerable.Range(1, count).Select(_ => new Seed(rnd.Next(int.MaxValue))).ToList();
    }
}
