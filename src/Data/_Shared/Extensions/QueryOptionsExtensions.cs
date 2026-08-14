using FlexQuery.NET.Models;

namespace Reaparr.Data;

public static class QueryOptionsExtensions
{
    public static bool HasFiltersApplied(this QueryOptions options) => options.Filter?.Filters.Any() ?? false;
}
