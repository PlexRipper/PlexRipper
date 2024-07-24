namespace FileSystem.Contracts;

public static class DriveInfoExtensions
{
    /// <summary>
    /// Nasty hack to check if we can read a drive mount
    /// </summary>
    /// <param name="driveInfo"></param>
    /// <returns> Returns true if we can read the directory, false otherwise</returns>
    public static bool CanRead(this DriveInfo driveInfo)
    {
        try
        {
            driveInfo.RootDirectory.GetDirectories();
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
    /// Nasty hack to check if we can write to a drive mount
    /// Source: https://stackoverflow.com/a/6371533/8205497
    /// </summary>
    /// <param name="driveInfo"></param>
    /// <returns></returns>
    public static bool CanWrite(this DriveInfo driveInfo)
    {
        try
        {
            using (
                File.Create(
                    Path.Combine(driveInfo.RootDirectory.FullName, Path.GetRandomFileName()),
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
