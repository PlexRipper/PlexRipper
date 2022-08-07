using Microsoft.AspNetCore.Mvc.Filters;
using PlexRipper.WebAPI.Common.Extensions;

namespace PlexRipper.WebAPI.Common;

public class ValidateFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
            context.Result = context.ModelState.ToBadRequestResult();
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}