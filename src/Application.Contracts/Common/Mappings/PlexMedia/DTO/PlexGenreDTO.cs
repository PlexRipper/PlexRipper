namespace Application.Contracts;

public record PlexGenreDTO
{
    public required int Id { get; set; }

    public required string Name { get; set; }
}
