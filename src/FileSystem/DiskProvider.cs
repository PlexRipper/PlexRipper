using FileSystem.Contracts;
using PlexRipper.FileSystem.Common;

namespace PlexRipper.FileSystem;

public sealed class DiskProvider : IDiskProvider
{
    private readonly HashSet<string> _setToRemove =
    [
        "boot",
        "bootmgr",
        "cache",
        "msocache",
        "recovery",
        "$recycle.bin",
        "recycler",
        "system volume information",
        "temporary internet files",
        "windows",
        // OS X
        ".fseventd",
        ".spotlight",
        ".trashes",
        ".vol",
        "cachedmessages",
        "caches",
        "trash",
        // QNAP
        ".@__thumb",
        // Synology
        "@eadir",
    ];

    public string GetParent(string path)
    {
        var di = new DirectoryInfo(path);

        if (di.Parent != null)
        {
            var parent = di.Parent.FullName;

            if (parent.Last() != Path.DirectorySeparatorChar)
                parent += Path.DirectorySeparatorChar;

            return parent;
        }

        if (!path.Equals("/"))
            return string.Empty;

        return string.Empty;
    }

    public Result<List<FileSystemModel>> GetFiles(string path)
    {
        var files = GetFileInfos(path);
        if (files.IsFailed)
            return files.ToResult();

        return files
            .Value.OrderBy(d => d.Name)
            .Select(d => new FileSystemModel
            {
                Name = d.Name,
                Path = d.FullName.GetActualCasing(),
                LastModified = d.LastWriteTimeUtc,
                Extension = d.Extension,
                Size = d.Length,
                Type = FileSystemEntityType.File,
            })
            .ToList();
    }

    public Result<List<FileSystemModel>> GetDirectories(string path)
    {
        var directories = GetDirectoryInfos(path);
        if (directories.IsFailed)
            return directories.ToResult();

        var list = directories
            .Value.OrderBy(d => d.Name)
            .Select(d => new FileSystemModel
            {
                Name = d.Name,
                Path = GetDirectoryPath(d.FullName.GetActualCasing()),
                LastModified = d.LastWriteTimeUtc,
                Type = FileSystemEntityType.Folder,
                Extension = d.Extension,
                Size = 0, // TODO maybe calculating this is a bit expensive, see if needed
            })
            .ToList();

        list.RemoveAll(d => _setToRemove.Contains(d.Name.ToLowerInvariant()));

        return Result.Ok(list);
    }

    public string GetDirectoryPath(string path)
    {
        if (path.Last() != Path.DirectorySeparatorChar)
            path += Path.DirectorySeparatorChar;

        return path;
    }

    public Result<List<DirectoryInfo>> GetDirectoryInfos(string path)
    {
        try
        {
            var di = new DirectoryInfo(path);

            return di.GetDirectories().ToList();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result<List<FileInfo>> GetFileInfos(string path)
    {
        try
        {
            var di = new DirectoryInfo(path);

            return di.GetFiles().ToList();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public List<IMount> GetMounts()
    {
        return GetAllMounts().Where(d => !IsSpecialMount(d)).ToList();
    }

    private bool IsSpecialMount(IMount mount) => false;

    private List<IMount> GetAllMounts()
    {
        return GetDriveInfoMounts()
            .Where(d =>
                d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Network || d.DriveType == DriveType.Removable
            )
            .Select(d => new DriveInfoMount(d))
            .Cast<IMount>()
            .ToList();
    }

    private List<DriveInfo> GetDriveInfoMounts()
    {
        return DriveInfo.GetDrives().Where(d => d.IsReady).ToList();
    }

    public string GetVolumeName(IMount mountInfo)
    {
        if (string.IsNullOrWhiteSpace(mountInfo.VolumeLabel))
            return mountInfo.Name;

        return $"{mountInfo.Name} ({mountInfo.VolumeLabel})";
    }
}
