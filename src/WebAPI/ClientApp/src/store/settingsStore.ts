import { defineStore, acceptHMRUpdate } from 'pinia';
import Log from 'consola';
import { Observable, of, Subject } from 'rxjs';
import { debounceTime, switchMap, tap } from 'rxjs/operators';
import type {
	ConfirmationSettingsDTO,
	DateTimeSettingsDTO,
	DebugSettingsDTO,
	DisplaySettingsDTO,
	DownloadManagerSettingsDTO,
	GeneralSettingsDTO,
	LanguageSettingsDTO,
	ServerSettingsDTO,
	SettingsModelDTO,
} from '@dto';

import { PlexMediaType, ViewMode } from '@dto';
import type { ISetupResult } from '@interfaces';
import { settingsApi } from '@api';

export const useSettingsStore = defineStore('SettingsStore', () => {
	// State
	const state = reactive<{
		generalSettings: GeneralSettingsDTO;
		debugSettings: DebugSettingsDTO;
		confirmationSettings: ConfirmationSettingsDTO;
		dateTimeSettings: DateTimeSettingsDTO;
		displaySettings: DisplaySettingsDTO;
		downloadManagerSettings: DownloadManagerSettingsDTO;
		languageSettings: LanguageSettingsDTO;
		serverSettings: ServerSettingsDTO;
	}>({
		generalSettings: {
			debugMode: false,
			activeAccountId: 0,
			firstTimeSetup: true,
			disableAnimatedBackground: false,
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
	});

	const _settingsUpdated = new Subject<SettingsModelDTO>();

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			// Send the settings to the server when they change
			_settingsUpdated
				.pipe(
					debounceTime(1000),
					switchMap((settings) => settingsApi.updateUserSettingsEndpoint(settings)),
				)
				.subscribe();

			return actions.refreshSettings().pipe(
				tap(() => {
					useSettingsStore().$subscribe((mutation, state) => {
						if (mutation.type) _settingsUpdated.next(state);
					});
				}),
				switchMap(() => of({ name: useSettingsStore.name, isSuccess: true })),
			);
		},
		refreshSettings(): Observable<SettingsModelDTO | null> {
			return settingsApi.getUserSettingsEndpoint().pipe(
				switchMap((settingsResult) => of(settingsResult?.value ?? null)),
				tap((settings) => {
					if (settings) {
						actions.setSettingsState(settings);
					}
				}),
			);
		},
		setSettingsState(settings: SettingsModelDTO) {
			state.generalSettings = settings.generalSettings;
			state.debugSettings = settings.debugSettings;
			state.confirmationSettings = settings.confirmationSettings;
			state.dateTimeSettings = settings.dateTimeSettings;
			state.displaySettings = settings.displaySettings;
			state.downloadManagerSettings = settings.downloadManagerSettings;
			state.languageSettings = settings.languageSettings;
			state.serverSettings = settings.serverSettings;
		},

		updateDownloadLimit(machineIdentifier: string, downloadLimit: number) {
			const i = state.serverSettings.data.findIndex((server) => server.machineIdentifier === machineIdentifier);
			if (i > -1) {
				state.serverSettings.data.splice(i, 1, {
					machineIdentifier,
					plexServerName: state.serverSettings.data[i].plexServerName,
					downloadSpeedLimit: downloadLimit,
				});
			}
		},
		updateDisplayMode(type: PlexMediaType, viewMode: ViewMode) {
			switch (type) {
				case PlexMediaType.Movie:
					state.displaySettings.movieViewMode = viewMode;
					break;
				case PlexMediaType.TvShow:
					state.displaySettings.tvShowViewMode = viewMode;
					break;
				default:
					Log.error('Could not set view mode for type' + type);
			}
		},
	};

	// Getters
	const getters = {
		debugMode: computed((): boolean => state.debugSettings.debugModeEnabled),
		getServerSettings: computed(
			() => (machineIdentifier?: string) =>
				machineIdentifier ? state.serverSettings.data.find((user) => user.machineIdentifier === machineIdentifier) : null,
		),
		shouldMaskServerNames: computed(
			(): boolean => state.debugSettings.debugModeEnabled && state.debugSettings.maskServerNames,
		),
		shouldMaskLibraryNames: computed(
			(): boolean => state.debugSettings.debugModeEnabled && state.debugSettings.maskLibraryNames,
		),
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useSettingsStore, import.meta.hot));
}
