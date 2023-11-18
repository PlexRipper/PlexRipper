using FluentResults;
using MediatR;

namespace Application.Contracts;

/// <summary>
/// Checks if an <see cref="PlexAccount"/> with the same username already exists.
/// </summary>
/// <param name="Username">The username to check for.</param>
/// <returns>true if the username is available.</returns>
public record CheckUsernameTaskRequest(string Username) : IRequest<Result<bool>>;