// noinspection AllyPlainJsInspection

import { MockConfig } from '@mock/interfaces';
import { SettingsModelDTO, ViewMode } from '@dto/mainApi';
import { checkConfig } from '@mock/mock-base';

export function generateSettings(config: MockConfig | null = null): SettingsModelDTO {
	config = checkConfig(config);

	return <SettingsModelDTO>{
		dateTimeSettings: {
			shortDateFormat: 'dd/MM/yyyy',
			longDateFormat: 'EEEE, dd MMMM yyyy',
			timeFormat: 'HH:mm:ss',
			timeZone: 'UTC',
			showRelativeDates: true,
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
		serverSettings: {
			data: [],
		},
		generalSettings: {
			firstTimeSetup: !!config.firstTimeSetup,
			activeAccountId: 0,
		},
		confirmationSettings: {
			askDownloadMovieConfirmation: true,
			askDownloadEpisodeConfirmation: true,
			askDownloadSeasonConfirmation: true,
			askDownloadTvShowConfirmation: true,
		},
	};
}
