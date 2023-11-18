using FluentResults;
using MediatR;

namespace Application.Contracts;

public class CheckUsernameTaskQuery : IRequest<Result<bool>>
{
    #region Constructors

    /// <summary>
    /// Checks if an <see cref="PlexAccount"/> with the same username already exists.
    /// </summary>
    /// <param name="username">The username to check for.</param>
    /// <returns>true if the username is available.</returns>
    public CheckUsernameTaskQuery(string username)
    {
        Username = username;
    }

    #endregion

    #region Properties

    public string Username { get; }

    #endregion
}