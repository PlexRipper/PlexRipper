namespace Application.Contracts;

public record PlexCountryDTO
{
    public required int Id { get; set; }

    public required string Name { get; set; }
}
