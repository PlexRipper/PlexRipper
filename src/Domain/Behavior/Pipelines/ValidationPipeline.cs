using FluentValidation;
using MediatR;
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
            // Pre
            var result = await _compositeValidator.ValidateAsync(request, cancellationToken);

            if (!result.IsValid)
            {
                var responseType = typeof(TResponse);
                var errorList = result.Errors.Select(s => s.ErrorMessage).ToList();
                Log.Error($"{responseType} - {errorList.Aggregate((acc, cur) => acc += string.Concat("_|_", cur))}");


                if (responseType.IsGenericType)
                {
                    var resultType = responseType.GetGenericArguments()[0];
                    var invalidResponseType = typeof(ValidationResponse<>).MakeGenericType(resultType);

                    var invalidResponse = Activator.CreateInstance(invalidResponseType, null, errorList) as TResponse;

                    return invalidResponse;
                }
            }

            return await next();
        }
    }
}
