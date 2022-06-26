using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Common.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IActionResult ToBadRequestResult(this ModelStateDictionary dictionary)
        {
            var error = new Error("Bad request error:");
            foreach (KeyValuePair<string, ModelStateEntry> keyValuePair in dictionary)
            {
                if (keyValuePair.Value.Children is not null)
                {
                    foreach (ModelStateEntry valueChild in keyValuePair.Value.Children)
                    {
                        foreach (ModelError childError in valueChild.Errors)
                        {
                            error.CausedBy(new Error(childError.ErrorMessage));
                        }
                    }
                }

                foreach (var modelError in keyValuePair.Value.Errors)
                {
                    error.CausedBy(new Error(modelError.ErrorMessage));
                }
            }

            var result = Result.Fail(error);
            return new BadRequestObjectResult(new ResultDTO
            {
                IsFailed = result.IsFailed,
                IsSuccess = result.IsSuccess,
                Errors = result.Errors,
                Successes = result.Successes,
            });
        }
    }
}