using PlexRipper.WebAPI.Common.DTO;

namespace PlexRipper.WebAPI.Common.Extensions;

public static class PlexMediaSlimDTOExtension
{
    public static PlexMediaSlimDTO SetIndex(this PlexMediaSlimDTO source)
    {
        for (var i = 0; i < source.Children.Count; i++)
        {
            source.Children[i].Index = i + 1;
            for (var j = 0; j < source.Children[i].Children.Count; j++)
                source.Children[i].Children[j].Index = j + 1;
        }

        return source;
    }

    public static List<PlexMediaSlimDTO> SetIndex(this List<PlexMediaSlimDTO> source)
    {
        for (var i = 0; i < source.Count; i++)
        {
            source[i].SetIndex();
            source[i].Index = i + 1;
        }

        return source;
    }
}