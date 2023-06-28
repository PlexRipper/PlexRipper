import { defineStore, acceptHMRUpdate } from 'pinia';
import Log from 'consola';
import { EMPTY, Observable, of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import {
	ConfirmationSettingsDTO,
	DateTimeSettingsDTO,
	DebugSettingsDTO,
	DisplaySettingsDTO,
	DownloadManagerSettingsDTO,
	GeneralSettingsDTO,
	LanguageSettingsDTO,
	PlexMediaType,
	ServerSettingsDTO,
	SettingsModelDTO,
	ViewMode,
} from '@dto/mainApi';
import ISetupResult from '@interfaces/service/ISetupResult';

export const useSettingsStore = defineStore('SettingsStore', {
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
		setup(): Observable<ISetupResult> {
			return EMPTY.pipe(switchMap(() => of({ name: useSettingsStore.name, isSuccess: true })));
		},
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
			if (i > -1) {
				this.serverSettings.data.splice(i, 1, {
					machineIdentifier,
					plexServerName: this.serverSettings.data[i].plexServerName,
					downloadSpeedLimit: downloadLimit,
				});
			}
		},
		updateDisplayMode(type: PlexMediaType, viewMode: ViewMode) {
			switch (type) {
				case PlexMediaType.Movie:
					this.displaySettings.movieViewMode = viewMode;
					break;
				case PlexMediaType.TvShow:
					this.displaySettings.tvShowViewMode = viewMode;
					break;
				default:
					Log.error('Could not set view mode for type' + type);
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

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useSettingsStore, import.meta.hot));
}
