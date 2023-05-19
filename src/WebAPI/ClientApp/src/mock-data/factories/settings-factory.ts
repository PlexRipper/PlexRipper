import { randBoolean } from '@ngneat/falso';
import { checkConfig, MockConfig } from '@mock';
import { PlexServerDTO, SettingsModelDTO, ViewMode } from '@dto/mainApi';

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
			shortDateFormat: 'dd/MM/yyyy',
			longDateFormat: 'EEEE, dd MMMM yyyy',
			timeFormat: 'HH:mm:ss',
			timeZone: 'UTC',
			showRelativeDates: randBoolean(),
		},
		displaySettings: {
			tvShowViewMode: ViewMode.Table,
			movieViewMode: ViewMode.Table,
		},
		downloadManagerSettings: {
			downloadSegments: 5,
		},
		languageSettings: {
			language: 'en-US',
		},

		generalSettings: {
			firstTimeSetup: validConfig.firstTimeSetup,
			debugMode: false,
			activeAccountId: 0,
		},
		confirmationSettings: {
			askDownloadMovieConfirmation: randBoolean(),
			askDownloadEpisodeConfirmation: randBoolean(),
			askDownloadSeasonConfirmation: randBoolean(),
			askDownloadTvShowConfirmation: randBoolean(),
		},
		serverSettings: {
			data: plexServers.map((x) => {
				return {
					machineIdentifier: x.machineIdentifier,
					plexServerName: x.name,
					downloadSpeedLimit: 0,
				};
			}),
		},
	};
}
