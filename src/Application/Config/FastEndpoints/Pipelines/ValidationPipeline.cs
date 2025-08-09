using FastEndpoints;
using FluentValidation;
using FluentValidation.Results;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace PlexRipper.Application;

public class ValidationPipeline<TRequest, TResponse> : ICommandMiddleware<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
    where TResponse : ResultBase, new()
{
    private readonly IReadOnlyList<IValidator<TRequest>> _validators;

    public ValidationPipeline(IEnumerable<IValidator<TRequest>> validators) => _validators = validators.ToList();

    public async Task<TResponse> ExecuteAsync(TRequest command, CommandDelegate<TResponse> next, CancellationToken ct)
    {
        if (_validators.Count > 0)
        {
            var ctx = new FluentValidation.ValidationContext<TRequest>(command);
            var failures = new List<ValidationFailure>();

            foreach (var v in _validators)
            {
                var r = await v.ValidateAsync(ctx, ct);
                if (!r.IsValid)
                    failures.AddRange(r.Errors);
            }

            if (failures.Count > 0)
                return new ValidationResult(failures).ToResult<TResponse>();
        }

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
