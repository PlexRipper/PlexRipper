namespace Reaparr.Application;

public static class AppUpdateCheckDTOMapper
{
    public static AppUpdateCheckDTO ToDTO(this AppUpdateCheckResult source) =>
        new()
        {
            IsUpdateAvailable = source.IsUpdateAvailable,
            NewestVersion = source.NewestVersion,
            CurrentVersion = source.CurrentVersion,
            ReleaseNotes = source
                .ReleaseNotes.Select(x => new ReleaseNoteDTO
                {
                    Version = x.Version,
                    Notes = x.Notes,
                    ReleaseDate = x.ReleaseDate,
                    IsDevRelease = x.IsDevRelease,
                })
                .ToList(),
        };
}
