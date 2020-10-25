using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common.Base
{
    public abstract class BaseHandler
    {
        private protected readonly IPlexRipperDbContext _dbContext;

        public BaseHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Result<T> ReturnResult<T>(T value, int id = 0) where T : BaseEntity
        {
            if (value != null)
            {
                return Result.Ok(value);
            }

            return Result.Fail(new Error($"Could not find an entity of {typeof(T)} with an id of {id}"));
        }

        public Result<List<T>> ReturnResult<T>(List<T> value)
        {
            if (value != null && value.Any())
            {
                return Result.Ok(value);
            }

            return Result.Fail(new Error($"Could not find entities of {typeof(T)}"));
        }


        public Result Validate<TQuery, TValidator>(TQuery request) where TValidator : AbstractValidator<TQuery>
        {
            var validator = (TValidator)Activator.CreateInstance(typeof(TValidator));
            var result = validator.Validate(request);
            return CreateResult(result);
        }

        public async Task<Result> ValidateAsync<TQuery, TValidator>(TQuery request) where TValidator : AbstractValidator<TQuery>
        {
            var validator = (TValidator)Activator.CreateInstance(typeof(TValidator));
            var result = await validator.ValidateAsync(request);
            return CreateResult(result);
        }

        private Result CreateResult(ValidationResult result)
        {
            if (!result.IsValid)
            {
                if (result.Errors.Count == 1)
                {
                    string msg = $"Validation Failure: {result.Errors.First().ErrorMessage}";
                    return Result.Fail(msg);
                }

                Error error = new Error("Validation Failure");

                foreach (var validationFailure in result.Errors)
                {
                    Log.Warning($"{validationFailure.ErrorMessage}");
                    error.Reasons.Add(new Error(validationFailure.ErrorMessage));
                }
                return Result.Fail(error);
            }
            return Result.Ok();
        }
    }
}
