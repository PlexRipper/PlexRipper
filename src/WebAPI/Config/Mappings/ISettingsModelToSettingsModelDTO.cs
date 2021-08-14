using AutoMapper;
using PlexRipper.Application.Common;
using PlexRipper.WebAPI.Common.DTO;

namespace PlexRipper.WebAPI.Config.Mappings
{
    public class ISettingsModelToSettingsModelDTO : ITypeConverter<ISettingsModel, SettingsModelDTO>
    {
        public SettingsModelDTO Convert(ISettingsModel source, SettingsModelDTO destination, ResolutionContext context)
        {
            return new SettingsModelDTO
            {
                FirstTimeSetup = source.FirstTimeSetup,
                AccountSettings = new AccountSettingsModelDTO
                {
                    ActiveAccountId = source.ActiveAccountId,
                },
                AdvancedSettings = new AdvancedSettingsModelDTO
                {
                    DownloadManager = new DownloadManagerModelDTO
                    {
                        DownloadSegments = source.DownloadSegments,
                    },
                },
                UserInterfaceSettings = new UserInterfaceSettingsModelDTO
                {
                    ConfirmationSettings = new ConfirmationSettingsModelDTO
                    {
                        AskDownloadMovieConfirmation = source.AskDownloadMovieConfirmation,
                        AskDownloadTvShowConfirmation = source.AskDownloadTvShowConfirmation,
                        AskDownloadSeasonConfirmation = source.AskDownloadSeasonConfirmation,
                        AskDownloadEpisodeConfirmation = source.AskDownloadEpisodeConfirmation,
                    },
                    DisplaySettings = new DisplaySettingsModelDTO
                    {
                        MovieViewMode = source.MovieViewMode,
                        TvShowViewMode = source.TvShowViewMode,
                    },
                    DateTimeSettings = new DateTimeModelDTO
                    {
                        TimeFormat = source.TimeFormat,
                        TimeZone = source.TimeZone,
                        ShortDateFormat = source.ShortDateFormat,
                        LongDateFormat = source.LongDateFormat,
                        ShowRelativeDates = source.ShowRelativeDates,
                    },
                },
            };
        }
    }
}