import Log from 'consola';
import { defineStore, acceptHMRUpdate } from 'pinia';
import { of, Subject, type Observable } from 'rxjs';
import { debounceTime, switchMap, tap } from 'rxjs/operators';
import { reactive, computed, toRefs } from 'vue';
import { PlexMediaType, type SettingsModelDTO, ViewMode } from '@dto';

import type { ISetupResult } from '@interfaces';
import { settingsApi } from '@api';
import { cloneDeep } from 'lodash-es';

export const useSettingsStore = defineStore('SettingsStore', () => {
	// State
	const defaultState: SettingsModelDTO = {
		generalSettings: {
			activeAccountId: 0,
			firstTimeSetup: true,
			disableAnimatedBackground: false,
			hideMediaFromOfflineServers: false,
			hideMediaFromOwnedServers: false,
			useLowQualityPosterImages: false,
			hasBeenInvitedToDiscord: false,
			hasAgreedToDisclaimer: false,
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
			allOverviewViewMode: PlexMediaType.TvShow,
		},
		downloadManagerSettings: {
			downloadSegments: 4,
			keepCompletedInDownloadFolder: false,
		},
		languageSettings: { language: 'en-US' },
		integrationsSettings: {
			downloadClientUsername: '',
			downloadClientPassword: '',
			reaparrApiKey: '',
			sonarr: {
				isConfigured: false,
				sonarrApiKey: '',
				sonarrBaseUrl: '',
			},
			radarr: {
				isConfigured: false,
				radarrApiKey: '',
				radarrBaseUrl: '',
			},
		},
		serverSettings: {
			data: [],
		},
	};

	const state = reactive<SettingsModelDTO>(cloneDeep(defaultState));
	const _settingsUpdated = new Subject<SettingsModelDTO>();

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.refreshSettings().pipe(
				tap(() => {
					// Send the settings to the server when they change
					_settingsUpdated
						.pipe(
							debounceTime(500),
							tap((settings) => Log.debug('Settings updated', settings)),
							switchMap((settings) => settingsApi.updateUserSettingsEndpoint(settings)),
						)
						.subscribe();

					useSettingsStore().$subscribe((mutation, state) => {
						if (mutation.type) {
							_settingsUpdated.next(state);
						}
					});
				},
				),
				switchMap(() => of({ name: 'useSettingsStore', isSuccess: true })),
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
		saveSettings: (): Observable<SettingsModelDTO | null> =>
			settingsApi.updateUserSettingsEndpoint(state).pipe(
				switchMap((settingsResult) => of(settingsResult?.value ?? null)),
				tap((settings) => {
					if (settings) {
						actions.setSettingsState(settings);
					}
				}),
			),
		setSettingsState(settings: SettingsModelDTO) {
			Object.assign(state.generalSettings, settings.generalSettings);
			Object.assign(state.debugSettings, settings.debugSettings);
			Object.assign(state.confirmationSettings, settings.confirmationSettings);
			Object.assign(state.dateTimeSettings, settings.dateTimeSettings);
			Object.assign(state.displaySettings, settings.displaySettings);
			Object.assign(state.downloadManagerSettings, settings.downloadManagerSettings);
			Object.assign(state.languageSettings, settings.languageSettings);

			// Arrays: replace contents, not the array instance
			state.serverSettings.data.splice(0, state.serverSettings.data.length, ...settings.serverSettings.data);

			// Keep containers stable, then merge deeply
			Object.assign(state.integrationsSettings, settings.integrationsSettings);
			Object.assign(state.integrationsSettings.sonarr, settings.integrationsSettings.sonarr);
			Object.assign(state.integrationsSettings.radarr, settings.integrationsSettings.radarr);
		},
		updateDownloadLimit(machineIdentifier: string, downloadLimit: number) {
			const i = state.serverSettings.data.findIndex((server) => server.machineIdentifier === machineIdentifier);
			if (i > -1) {
				state.serverSettings.data.splice(i, 1, {
					machineIdentifier,
					plexServerName: state.serverSettings.data[i]!.plexServerName,
					downloadSpeedLimit: downloadLimit,
					hidden: state.serverSettings.data[i]!.hidden,
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
		isServerVisible(machineIdentifier: string): boolean {
			return !(actions.getServerSettings(machineIdentifier)?.hidden ?? false);
		},
		getServerSettings: (machineIdentifier?: string) =>
			machineIdentifier ? state.serverSettings.data.find((user) => user.machineIdentifier === machineIdentifier) : null,
		/**
     * Returns the server name for the given machine identifier.
     * If the debug mode is enabled, the server name will be masked.
     * If there is no custom server name, an empty string will be returned.
     * @param machineIdentifier The machine identifier of the server.
     */
		getServerName: (machineIdentifier: string) =>
			getters.shouldMaskServerNames.value
				? '**MASKED**'
				: actions.getServerSettings(machineIdentifier)?.plexServerName ?? '',

		isConfirmationEnabled: (type: PlexMediaType) => {
			switch (type) {
				case PlexMediaType.Movie:
					return state.confirmationSettings.askDownloadMovieConfirmation;
				case PlexMediaType.TvShow:
					return state.confirmationSettings.askDownloadTvShowConfirmation;
				case PlexMediaType.Season:
					return state.confirmationSettings.askDownloadSeasonConfirmation;
				case PlexMediaType.Episode:
					return state.confirmationSettings.askDownloadEpisodeConfirmation;
				default:
					return true;
			}
		},
		$reset() {
			_settingsUpdated.complete();
			actions.setSettingsState(cloneDeep(defaultState));
		},
	};

	// Getters
	const getters = {
		debugMode: computed((): boolean => state.debugSettings.debugModeEnabled),
		shouldMaskServerNames: computed((): boolean => state.debugSettings.debugModeEnabled && state.debugSettings.maskServerNames),
		shouldMaskLibraryNames: computed((): boolean => state.debugSettings.debugModeEnabled && state.debugSettings.maskLibraryNames),
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
