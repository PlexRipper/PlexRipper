using Microsoft.AspNetCore.Mvc.Filters;
using PlexRipper.WebAPI.Common.Extensions;

namespace PlexRipper.WebAPI.Common
{
    public class ValidateFilter : Microsoft.AspNetCore.Mvc.Filters.IActionFilter
    {
        public void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = context.ModelState.ToBadRequestResult();
            }
        }

        public void OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext context) { }
}
    }
