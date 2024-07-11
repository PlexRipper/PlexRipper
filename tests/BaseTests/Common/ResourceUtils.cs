using System.Reflection;

namespace PlexRipper.BaseTests;

/// <summary>
/// Source: https://korzh.com/blog/embedded-resources-testing-projects
/// </summary>
public static class ResourceUtils
{
    public static Stream GetResourceStream(this Assembly assembly, string resourceFolder, string resourceFileName)
    {
        var nameParts = assembly.FullName.Split(',');

        var resourceName = nameParts[0] + "." + resourceFolder + "." + resourceFileName;

        var resources = new List<string>(assembly.GetManifestResourceNames());
        if (resources.Contains(resourceName))
            return assembly.GetManifestResourceStream(resourceName);
        else
            return null;
    }

    public static string GetResourceAsString(this Assembly assembly, string folder, string fileName)
    {
        string fileContent;
        using (var sr = new StreamReader(GetResourceStream(assembly, folder, fileName)))
        {
            fileContent = sr.ReadToEnd();
        }

        return fileContent;
    }

    public static Stream GetResourceStream(this Type type, string resourceFolder, string resourceFileName)
    {
        var assembly = type.GetTypeInfo().Assembly;
        return assembly.GetResourceStream(resourceFolder, resourceFileName);
    }

    public static string GetResourceAsString(this Type type, string folder, string fileName)
    {
        var assembly = type.GetTypeInfo().Assembly;
        return assembly.GetResourceAsString(folder, fileName);
    }
}

public class ResourceUtilsException : Exception
{
    public ResourceUtilsException(string message)
        : base(message) { }
}
