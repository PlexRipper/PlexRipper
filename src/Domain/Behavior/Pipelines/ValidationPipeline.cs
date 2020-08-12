using FluentResults;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Domain.Behavior.Pipelines
{
    public class ValidationPipeline<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TResponse : ResultBase, new()
    {
        private readonly IValidator<TRequest> _compositeValidator;

        public ValidationPipeline(IValidator<TRequest> compositeValidator)
        {
            _compositeValidator = compositeValidator;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var fluentValidationResult = await _compositeValidator.ValidateAsync(request, cancellationToken);

            if (!fluentValidationResult.IsValid)
            {
                var result = new TResponse();

                foreach (var reason in fluentValidationResult.Errors)
                {
                    result.Reasons.Add(new Error(reason.ErrorMessage));
                }

                return result;
            }

            return await next();
        }
    }
}