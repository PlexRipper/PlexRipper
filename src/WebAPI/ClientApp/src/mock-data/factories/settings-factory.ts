import { checkConfig, type MockConfig } from '@mock';
import { type PlexServerDTO, type SettingsModelDTO, ViewMode } from '@dto';

export function generateSettingsModel({
	plexServers = [],
	config = {},
}: {
	plexServers?: PlexServerDTO[];
	config?: Partial<MockConfig>;
}): SettingsModelDTO {
	const validConfig = checkConfig(config);

	return <SettingsModelDTO>{
		dateTimeSettings: {
			longDateFormat: 'EEEE, dd MMMM yyyy',
			shortDateFormat: 'dd/MM/yyyy',
			showRelativeDates: false,
			timeFormat: 'HH:mm:ss',
			timeZone: 'UTC',
		},
		debugSettings: {
			debugModeEnabled: false,
			maskLibraryNames: false,
			maskServerNames: false,
		},
		displaySettings: {
			movieViewMode: ViewMode.Poster,
			tvShowViewMode: ViewMode.Poster,
		},
		downloadManagerSettings: {
			downloadSegments: 4,
		},
		languageSettings: {
			language: 'en-US',
		},
		generalSettings: {
			firstTimeSetup: validConfig.firstTimeSetup,
			debugMode: true,
			activeAccountId: 0,
			disableAnimatedBackground: true,
		},
		confirmationSettings: {
			askDownloadEpisodeConfirmation: true,
			askDownloadMovieConfirmation: true,
			askDownloadSeasonConfirmation: true,
			askDownloadTvShowConfirmation: true,
		},
		serverSettings: {
			data: plexServers.map((x) => {
				return {
					machineIdentifier: x.machineIdentifier,
					plexServerName: '',
					downloadSpeedLimit: 0,
				};
			}),
		},
	};
}
