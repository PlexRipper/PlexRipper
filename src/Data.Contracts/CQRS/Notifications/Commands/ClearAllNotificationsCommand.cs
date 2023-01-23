using FluentResults;
using MediatR;

namespace Data.Contracts;

public class ClearAllNotificationsCommand : IRequest<Result<int>> { }