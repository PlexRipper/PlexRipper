namespace Reaparr.Application;

public record JobKeyV2
{
    [SetsRequiredMembers]
    public JobKeyV2(string name, JobTypes type)
    {
        Name = name;
        Type = type;
    }
    
    public required string Name { get; init; }

    public required JobTypes Type { get; init; }
}