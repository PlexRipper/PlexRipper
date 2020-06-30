using FluentValidation;
using MediatR;
using PlexRipper.Domain.Validation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Domain.Behavior.Pipelines
{
    public class ValidationPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TResponse : class
        where TRequest : IRequest<TResponse>
    {
        private readonly IValidator<TRequest> _compositeValidator;

        public ValidationPipeline(IValidator<TRequest> compositeValidator)
        {
            _compositeValidator = compositeValidator;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var result = await _compositeValidator.ValidateAsync(request, cancellationToken);

            if (!result.IsValid)
            {
                Log.Error(result.Errors.Select(s => s.ErrorMessage).Aggregate((acc, cur) => acc += string.Concat("_|_", cur))
                );

                var responseType = typeof(TResponse);

                if (responseType.IsGenericType)
                {
                    var resultType = responseType.GetGenericArguments()[0];
                    var invalidResponseType = typeof(ValidateableResponse<>).MakeGenericType(resultType);

                    return Activator.CreateInstance(invalidResponseType, null, result.Errors.Select(s => s.ErrorMessage).ToList()) as TResponse;

                }
            }

            var response = await next();

            return response;
        }
    }
}
