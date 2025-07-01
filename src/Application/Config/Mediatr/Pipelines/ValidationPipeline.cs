using FluentValidation;

namespace PlexRipper.Application;

public class ValidationPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : ResultBase, new()
{
    private readonly IValidator<TRequest> _compositeValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationPipeline{TRequest, TResponse}"/> class.
    /// Note: Every Query or Command should have a Validator class added otherwise it will silently fail the execution.
    /// </summary>
    /// <param name="compositeValidator"></param>
    public ValidationPipeline(IValidator<TRequest> compositeValidator)
    {
        _compositeValidator = compositeValidator;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var fluentValidationResult = await _compositeValidator.ValidateAsync(request, cancellationToken);

        if (!fluentValidationResult.IsValid)
            return fluentValidationResult.ToResult<TResponse>();

        // Encapsulate all handlers with this try/catch block as not to do this in every handler itself.
        try
        {
            return await next();
        }
        catch (Exception e)
        {
            var result = new TResponse();
            result.Reasons.Add(new ExceptionalError(e));
            return result;
        }
    }
}
