using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Domain.Behavior.Pipelines
{
    public class ValidationPipeline<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
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
            ValidationResult result = await _compositeValidator.ValidateAsync(request, cancellationToken);

            if (!result.IsValid)
            {
                Error error = new Error();
                var responseType = typeof(TResponse);

                foreach (var validationFailure in result.Errors)
                {
                    Log.Warning($"{responseType} - {validationFailure.ErrorMessage}");
                    error.Reasons.Add(new Error(validationFailure.ErrorMessage));
                }

                var x = Result.Fail(error);

                //var z = (TResponse)Activator.CreateInstance(typeof(TResponse));

                if (responseType.IsGenericType)
                {
                    var resultType = responseType.GetGenericArguments()[0];
                    var invalidResponseType = typeof(Result<>).MakeGenericType(resultType);

                    // TODO This will always return null, needs a fix
                    // https://github.com/altmann/FluentResults/issues/54
                    var f = Result.Fail(error) as TResponse;
                    return f;
                    //var invalidResponse =
                    //    Activator.CreateInstance(invalidResponseType, null, result.Errors.Select(s => s.ErrorMessage).ToList()) as TResponse;

                    //return invalidResponse;
                }

            }

            return await next();
        }
    }


}
