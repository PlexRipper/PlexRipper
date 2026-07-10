using Microsoft.Extensions.DependencyInjection;
using Moq;
using Reaparr.Application;
using Reaparr.BaseTests.MockDatabase;
using Reaparr.Data.Contracts;
using TUnit.Core.Interfaces;

namespace Reaparr.BaseTests;

[NotInParallel]
public abstract class BaseEndpointUnitTest<TEndpoint, TRequest, TResultDto>
    : BaseEndpointUnitTestBase
    where TEndpoint : class, IEndpoint<TRequest, TResultDto>
    where TRequest : notnull
    where TResultDto : class
{
    private EndpointTestResult<TResultDto>? _cachedResult;

    protected async Task<EndpointTestResult<TResultDto>> TestEndpointHandleAsync(
        TRequest request,
        Action<IServiceCollection>? extraServices = null
    )
    {
        return await TestEndpointHandleAsyncInternal(
            request,
            typeof(TEndpoint),
            typeof(TRequest),
            typeof(TResultDto),
            extraServices
        ) as EndpointTestResult<TResultDto>
               ?? throw new InvalidOperationException($"Invalid endpoint test result type for {typeof(TEndpoint).Name}");
    }

    protected async Task<EndpointTestResult<TResultDto>> TestEndpointHandleAsync(
        TRequest request,
        Action<IServiceCollection> extraServices,
        Action<EndpointTestResult<TResultDto>> configureTest
    )
    {
        var result = await TestEndpointHandleAsync(request, extraServices);
        configureTest(result);
        return result;
    }
}