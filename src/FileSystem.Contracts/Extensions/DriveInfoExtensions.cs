namespace FileSystem.Contracts.Extensions;

public static class DirectoryInfoExtensions
{
    /// <summary>
    /// Nasty hack to check if we can read a directory
    /// </summary>
    /// <param name="directoryInfo"></param>
    /// <returns> Returns true if we can read the directory, false otherwise</returns>
    public static bool CanRead(this DirectoryInfo directoryInfo)
    {
        try
        {
            directoryInfo.GetDirectories();
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Nasty hack to check if we can write to a directory
    /// Source: https://stackoverflow.com/a/6371533/8205497
    /// </summary>
    /// <param name="directoryInfo"></param>
    /// <returns></returns>
    public static bool CanWrite(this DirectoryInfo directoryInfo)
    {
        try
        {
            using (
                File.Create(
                    Path.Combine(directoryInfo.FullName, Path.GetRandomFileName()),
                    1,
                    FileOptions.DeleteOnClose
                )
            ) { }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
