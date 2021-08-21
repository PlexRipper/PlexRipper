using AutoMapper;
using PlexRipper.Application.Common;
using PlexRipper.Settings.Models;
using PlexRipper.WebAPI.Common.DTO;

namespace PlexRipper.WebAPI.Config.Mappings
{
    public class SettingsModelDTOToISettingsModel : ITypeConverter<SettingsModelDTO, ISettingsModel>
    {
        public ISettingsModel Convert(SettingsModelDTO source, ISettingsModel destination, ResolutionContext context)
        {
            return new SettingsModel
            {
                FirstTimeSetup = source.FirstTimeSetup,
                ActiveAccountId = source.AccountSettings.ActiveAccountId,
                DownloadSegments = source.AdvancedSettings.DownloadManager.DownloadSegments,
                AskDownloadMovieConfirmation = source.UserInterfaceSettings.ConfirmationSettings.AskDownloadMovieConfirmation,
                AskDownloadTvShowConfirmation = source.UserInterfaceSettings.ConfirmationSettings.AskDownloadTvShowConfirmation,
                AskDownloadSeasonConfirmation = source.UserInterfaceSettings.ConfirmationSettings.AskDownloadSeasonConfirmation,
                AskDownloadEpisodeConfirmation = source.UserInterfaceSettings.ConfirmationSettings.AskDownloadEpisodeConfirmation,
                MovieViewMode = source.UserInterfaceSettings.DisplaySettings.MovieViewMode,
                TvShowViewMode = source.UserInterfaceSettings.DisplaySettings.TvShowViewMode,
                TimeFormat = source.UserInterfaceSettings.DateTimeSettings.TimeFormat,
                TimeZone = source.UserInterfaceSettings.DateTimeSettings.TimeZone,
                ShortDateFormat = source.UserInterfaceSettings.DateTimeSettings.ShortDateFormat,
                LongDateFormat = source.UserInterfaceSettings.DateTimeSettings.LongDateFormat,
                ShowRelativeDates = source.UserInterfaceSettings.DateTimeSettings.ShowRelativeDates,
            };
        }
    }
}