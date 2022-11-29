using Environment;

namespace PlexRipper.Domain;

public static class StringExtensions
{
    private static Random random = new();

    public static string GetActualCasing(this string path)
    {
        if (OsInfo.IsNotWindows || path.StartsWith("\\"))
            return path;

        if (Directory.Exists(path) && (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            return GetProperCapitalization(new DirectoryInfo(path));

        var fileInfo = new FileInfo(path);
        var dirInfo = fileInfo.Directory;

        var fileName = fileInfo.Name;

        if (dirInfo != null && fileInfo.Exists)
            fileName = dirInfo.GetFiles(fileInfo.Name)[0].Name;

        return Path.Combine(GetProperCapitalization(dirInfo), fileName);
    }

    public static string RandomString(int length, bool allowNumbers = false, bool allowCapitalLetters = false)
    {
        var chars = "abcdefghijklmnopqrstuvwxyz";

        if (allowCapitalLetters)
            chars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        if (allowNumbers)
            chars += "0123456789";

        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string GetProperCapitalization(DirectoryInfo dirInfo)
    {
        var parentDirInfo = dirInfo.Parent;
        if (parentDirInfo == null)
        {
            // Drive letter
            return dirInfo.Name.ToUpper();
        }

        var folderName = dirInfo.Name;

        if (dirInfo.Exists)
            folderName = parentDirInfo.GetDirectories(dirInfo.Name)[0].Name;

        return Path.Combine(GetProperCapitalization(parentDirInfo), folderName);
    }
}