#if NETSTANDARD2_0
// C# 9+ init-only setters and record types require IsExternalInit on netstandard2.0.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

// HashCode.Combine was introduced in .NET Core 2.1 / .NET Standard 2.1.
// Provide a minimal polyfill for the netstandard2.0 build.
namespace System
{
    internal static class HashCode
    {
        public static int Combine<T1, T2, T3, T4, T5, T6, T7>(
            T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
        {
            unchecked
            {
                var h = 17;
                h = h * 31 + (v1 is null ? 0 : v1.GetHashCode());
                h = h * 31 + (v2 is null ? 0 : v2.GetHashCode());
                h = h * 31 + (v3 is null ? 0 : v3.GetHashCode());
                h = h * 31 + (v4 is null ? 0 : v4.GetHashCode());
                h = h * 31 + (v5 is null ? 0 : v5.GetHashCode());
                h = h * 31 + (v6 is null ? 0 : v6.GetHashCode());
                h = h * 31 + (v7 is null ? 0 : v7.GetHashCode());
                return h;
            }
        }
    }
}
#endif
