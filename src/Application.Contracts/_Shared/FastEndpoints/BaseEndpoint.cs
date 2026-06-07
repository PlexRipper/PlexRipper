namespace Reaparr.Application.Contracts;

public abstract class BaseEndpoint<TRequest> : Endpoint<TRequest, BaseResultDTO>
    where TRequest : class
{
    public abstract string EndpointPath { get; }
}

// ReSharper disable once InconsistentNaming
public abstract class BaseEndpoint<TRequest, TDTO> : BaseEndpoint<TRequest>
    where TRequest : class;

public abstract class BaseEndpointWithoutRequest : EndpointWithoutRequest<BaseResultDTO>
{
    public abstract string EndpointPath { get; }
}

public abstract class BaseEndpointWithoutRequest<TResponse> : BaseEndpointWithoutRequest;
