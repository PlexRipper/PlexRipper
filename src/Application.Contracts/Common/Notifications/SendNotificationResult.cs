using FastEndpoints;
using FluentResults;

namespace Application.Contracts;

public record SendNotificationResult(Result Result) : IEvent;
