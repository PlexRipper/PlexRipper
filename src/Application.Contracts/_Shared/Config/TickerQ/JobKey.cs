using System.Diagnostics.CodeAnalysis;

namespace Reaparr.Application.Contracts;

public record JobKey
{
    [SetsRequiredMembers]
    public JobKey(string name, JobTypes type)
    {
        Name = name;
        Type = type;
    }

    public required string Name { get; init; }

    public required JobTypes Type { get; init; }
}
