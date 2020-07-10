namespace PlexRipper.Application.Common.Interfaces.FileSystem
{
    public interface IFileSystem
    {
        string RootDirectory { get; }
        string ConfigDirectory { get; }
    }
}