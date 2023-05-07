import * as Factory from 'factory.ts';
import { randBoolean } from '@ngneat/falso';
import { checkConfig, MockConfig, resetSeed } from '@mock';
import { PlexServerDTO, SettingsModelDTO, ViewMode } from '@dto/mainApi';

export function generateSettingsModel({
	plexServers = [],
	config = {},
}: {
	plexServers?: PlexServerDTO[];
	config?: Partial<MockConfig>;
}): SettingsModelDTO {
	const validConfig = checkConfig(config);

	const settingsModelDTOFactory = Factory.Sync.makeFactory<SettingsModelDTO>(() => {
		resetSeed();

		return {
			dateTimeSettings: {
				shortDateFormat: 'dd/MM/yyyy',
				longDateFormat: 'EEEE, dd MMMM yyyy',
				timeFormat: 'HH:mm:ss',
				timeZone: 'UTC',
				showRelativeDates: randBoolean(),
			},
			displaySettings: {
				tvShowViewMode: ViewMode.Overview,
				movieViewMode: ViewMode.Overview,
			},
			downloadManagerSettings: {
				downloadSegments: 5,
			},
			languageSettings: {
				language: 'en-US',
			},

			generalSettings: {
				firstTimeSetup: validConfig.firstTimeSetup,
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
	});

	return settingsModelDTOFactory.build();
}
