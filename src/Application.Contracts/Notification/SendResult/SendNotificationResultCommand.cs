using FluentResults;
using MediatR;

namespace Application.Contracts;

public record SendNotificationResult(Result Result) : INotification;