namespace Reaparr.Application;

/// <summary>
/// Represents a queued immediate patch request containing changed node ids for a scoped root.
/// </summary>
public sealed record ImmediatePatchRequest(ProgressScopeKey Scope, IReadOnlyCollection<Guid> ChangedNodeIds);
