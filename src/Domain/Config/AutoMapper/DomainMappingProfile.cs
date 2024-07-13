using AutoMapper;

namespace PlexRipper.Domain.Config.AutoMapper;

/// <summary>
/// Contains all generic primitive mappings and entity mappings.
/// </summary>
public class DomainMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainMappingProfile"/> class.
    /// </summary>
    public DomainMappingProfile()
    {
        // string <-> Guid
        CreateMap<string, Guid>()
            .ConvertUsing(s => Guid.Parse(s));
        CreateMap<Guid, string>().ConvertUsing(g => g.ToString("N"));

        // long <-> DateTime
        CreateMap<long, DateTime>()
            .ConvertUsing(g => DateTimeOffset.FromUnixTimeSeconds(g).DateTime.ToUniversalTime());
        CreateMap<DateTime, long>().ConvertUsing(g => new DateTimeOffset(g).ToUnixTimeSeconds());

        // string <-> int
        CreateMap<string, int>()
            .ConvertUsing(g => ToInt(g));
        CreateMap<int, string>().ConvertUsing(g => g.ToString());
    }

    private static int ToInt(string stringInt) => int.TryParse(stringInt, out var x) ? x : 0;
}
