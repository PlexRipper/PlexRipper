using FileSystem.Contracts;

namespace PlexRipper.FileSystem.Common;

public class DriveInfoMount : IMount
{
    private readonly DriveInfo _driveInfo;

    private readonly DriveType _driveType;

    public DriveInfoMount(DriveInfo driveInfo, DriveType driveType = DriveType.Unknown)
    {
        _driveInfo = driveInfo;
        _driveType = driveType;
    }

    public long AvailableFreeSpace => _driveInfo.AvailableFreeSpace;

    public string DriveFormat => _driveInfo.DriveFormat;

    public DriveType DriveType
    {
        get
        {
            if (_driveType != DriveType.Unknown)
                return _driveType;

            return _driveInfo.DriveType;
        }
    }

    public bool IsReady => _driveInfo.IsReady;

    public string Name => _driveInfo.Name;

    public string RootDirectory => _driveInfo.RootDirectory.FullName;

    public long TotalFreeSpace => _driveInfo.TotalFreeSpace;

    public long TotalSize => _driveInfo.TotalSize;

    public string VolumeLabel => _driveInfo.VolumeLabel;

    public string VolumeName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(VolumeLabel) || VolumeLabel.StartsWith("UUID=") || Name == VolumeLabel)
                return Name;

            return $"{Name} ({VolumeLabel})";
        }
    }
}
