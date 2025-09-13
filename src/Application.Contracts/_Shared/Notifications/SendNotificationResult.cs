using FastEndpoints;
using FluentResults;

namespace Reaparr.Application.Contracts;

public record SendNotificationResult(Result Result) : IEvent;
