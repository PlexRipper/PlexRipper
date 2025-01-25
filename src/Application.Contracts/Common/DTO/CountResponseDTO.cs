namespace Application.Contracts;

/// <summary>
/// The front-end doesn't like primitive types being sent directly as a value in the ResultDTO so we wrap it here.
/// </summary>
/// <param name="Count"> The singular value to return.</param>
public record CountResponseDTO(int Count);
