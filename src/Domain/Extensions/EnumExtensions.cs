using System;
using System.Collections.Generic;
using FluentResultExtensions.lib;
using Logging;

namespace PlexRipper.Domain
{
    public static class EnumExtensions
    {
        public static Dictionary<string, ViewMode> ToViewModeDict => new()
        {
            {
                Enum.GetName(ViewMode.None)!, ViewMode.None
            },
            {
                Enum.GetName(ViewMode.Overview)!, ViewMode.Overview
            },
            {
                Enum.GetName(ViewMode.Poster)!, ViewMode.Poster
            },
            {
                Enum.GetName(ViewMode.Table)!, ViewMode.Table
            },
        };

        public static ViewMode ToViewMode(this string value)
        {
            if (ToViewModeDict.ContainsKey(value))
            {
                return ToViewModeDict[value];
            }

            Log.Warning($"Could not convert string {value} to enum {nameof(ViewMode)}");
            return ViewMode.None;
        }
    }
}