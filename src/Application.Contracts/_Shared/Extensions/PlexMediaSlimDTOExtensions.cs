namespace Reaparr.Application.Contracts;

public static class PlexMediaSlimDTOExtensions
{
    public static void SetComparisonState(this PlexMediaSlimDTO source, PlexMediaComparisonState state)
    {
        source.ComparisonId = state.ToComparisonId();
    }
}
