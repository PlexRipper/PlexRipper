using FluentResults;
using System.Collections.Generic;
using System.Linq;

namespace PlexRipper.Domain.Base
{
    public class BaseHandler
    {
        public Result<T> ReturnResult<T>(T value, int id = 0)
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

    }
}
