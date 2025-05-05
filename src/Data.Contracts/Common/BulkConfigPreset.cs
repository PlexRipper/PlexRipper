using EFCore.BulkExtensions;

namespace Data.Contracts;

public static class BulkConfigPreset
{
    public static BulkConfig Default { get; } =
        new()
        {
            BatchSize = 500,
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
            CalculateStats = true,
        };
}
