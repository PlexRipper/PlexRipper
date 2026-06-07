namespace Reaparr.Application.Contracts;

public abstract class BaseEndpoint<TRequest> : Endpoint<TRequest, BaseResultDTO>
    where TRequest : class
{ }

// ReSharper disable once InconsistentNaming
public abstract class BaseEndpoint<TRequest, TDTO> : BaseEndpoint<TRequest>
    where TRequest : class
{ }

public abstract class BaseEndpointWithoutRequest : EndpointWithoutRequest<BaseResultDTO>
{ }

public abstract class BaseEndpointWithoutRequest<TResponse> : BaseEndpointWithoutRequest
{ }
