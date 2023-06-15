import { defineStore } from 'pinia';
import {
	ConfirmationSettingsDTO,
	DateTimeSettingsDTO,
	DebugSettingsDTO,
	DisplaySettingsDTO,
	DownloadManagerSettingsDTO,
	GeneralSettingsDTO,
	LanguageSettingsDTO,
	ServerSettingsDTO,
	SettingsModelDTO,
	ViewMode,
} from '@dto/mainApi';

export const useSettingsStore = defineStore('settingsStore', {
	state: (): {
		generalSettings: GeneralSettingsDTO;
		debugSettings: DebugSettingsDTO;
		confirmationSettings: ConfirmationSettingsDTO;
		dateTimeSettings: DateTimeSettingsDTO;
		displaySettings: DisplaySettingsDTO;
		downloadManagerSettings: DownloadManagerSettingsDTO;
		languageSettings: LanguageSettingsDTO;
		serverSettings: ServerSettingsDTO;
	} => ({
		generalSettings: {
			debugMode: false,
			activeAccountId: 0,
			firstTimeSetup: true,
		},
		debugSettings: { debugModeEnabled: false, maskLibraryNames: false, maskServerNames: false },
		confirmationSettings: {
			askDownloadEpisodeConfirmation: true,
			askDownloadMovieConfirmation: true,
			askDownloadSeasonConfirmation: true,
			askDownloadTvShowConfirmation: true,
		},
		dateTimeSettings: {
			longDateFormat: 'EEEE, dd MMMM yyyy',
			shortDateFormat: 'dd/MM/yyyy',
			showRelativeDates: false,
			timeFormat: 'HH:mm:ss',
			timeZone: 'UTC',
		},
		displaySettings: {
			movieViewMode: ViewMode.Poster,
			tvShowViewMode: ViewMode.Poster,
		},
		downloadManagerSettings: { downloadSegments: 4 },
		languageSettings: { language: 'en-US' },
		serverSettings: {
			data: [],
		},
	}),
	actions: {
		setSettingsState(settings: SettingsModelDTO) {
			this.generalSettings = settings.generalSettings;
			this.debugSettings = settings.debugSettings;
			this.confirmationSettings = settings.confirmationSettings;
			this.dateTimeSettings = settings.dateTimeSettings;
			this.displaySettings = settings.displaySettings;
			this.downloadManagerSettings = settings.downloadManagerSettings;
			this.languageSettings = settings.languageSettings;
			this.serverSettings = settings.serverSettings;
		},
		updateDownloadLimit(machineIdentifier: string, downloadLimit: number) {
			const i = this.serverSettings.data.findIndex((server) => server.machineIdentifier === machineIdentifier);
			if (i === -1) {
				this.serverSettings.data[i].downloadSpeedLimit = downloadLimit;
			}
		},
	},
	getters: {
		debugMode: (state) => state.debugSettings.debugModeEnabled,
		getServerSettings: (state) => {
			return (machineIdentifier?: string) =>
				machineIdentifier ? state.serverSettings.data.find((user) => user.machineIdentifier === machineIdentifier) : null;
		},
	},
});
