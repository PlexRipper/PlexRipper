import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, toRefs, computed } from 'vue';
import { switchMap, tap, catchError } from 'rxjs/operators';
import { type Observable, of, forkJoin } from 'rxjs';
import { cloneDeep } from 'lodash-es';
import { type BaseResultDTO, TestConnectionStatus } from '@dto';
import { integrationApi } from '@api';
import { useSettingsStore } from '@store';
import { StoreNames, type ISetupResult } from '@interfaces';

interface IIntegrationStoreState {
	sonarr: IIntegrationState;
	radarr: IIntegrationState;
}

interface IIntegrationState {
	step: number;
	isTesting: boolean;
	isConfiguring: boolean;
	testSuccess: boolean | null;
	testStatus: TestConnectionStatus | null;
	configuringSuccess: boolean | null;
	error: BaseResultDTO | null;
}

export const useIntegrationStore = defineStore(StoreNames.IntegrationStore, () => {
	const defaultState: IIntegrationStoreState = {
		sonarr: {
			step: 1,
			isTesting: false,
			isConfiguring: false,
			testSuccess: null,
			testStatus: null,
			configuringSuccess: null,
			error: null,
		},
		radarr: {
			step: 1,
			isTesting: false,
			isConfiguring: false,
			testSuccess: null,
			testStatus: null,
			configuringSuccess: null,
			error: null,
		},
	};

	const state = reactive<IIntegrationStoreState>(cloneDeep(defaultState));
	const settingsStore = useSettingsStore();

	// Validation helpers
	const isValidUrl = (url: string | undefined | null): boolean => {
		if (!url?.trim()) return false;
		try {
			new URL(url.trim());
			return true;
		} catch {
			return false;
		}
	};

	const isValidApiKey = (apiKey: string | undefined | null): boolean => {
		if (!apiKey?.trim()) return false;
		const trimmedKey = apiKey.trim();
		// API key should be 32 characters and hexadecimal
		return trimmedKey.length === 32 && /^[a-f0-9]+$/i.test(trimmedKey);
	};

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			const sonarrSettings = settingsStore.integrationsSettings.sonarr;
			const radarrSettings = settingsStore.integrationsSettings.radarr;

			const testObservables: Observable<{ integration: string; isSuccess: boolean }>[] = [];

			// Test Sonarr if configured
			if (sonarrSettings.isConfigured && sonarrSettings.sonarrBaseUrl && sonarrSettings.sonarrApiKey) {
				testObservables.push(
					integrationApi.testConnectionToSonarrEndpoint({
						url: sonarrSettings.sonarrBaseUrl,
						apiKey: sonarrSettings.sonarrApiKey,
					}).pipe(
						tap((response) => {
							if (!response.isSuccess || response.value?.result !== TestConnectionStatus.Success) {
								showErrorNotification('Failed to connect to Sonarr. Please check your integration settings.');
								state.sonarr.testSuccess = false;
								state.sonarr.testStatus = response.value?.result ?? TestConnectionStatus.Unknown;
								state.sonarr.step = 1;
							} else {
								// If Sonarr connection is successful, set step to 3 to show as completed
								state.sonarr.step = 3;
								state.sonarr.testSuccess = true;
								state.sonarr.configuringSuccess = settingsStore.integrationsSettings.sonarr.isConfigured;
							}
						}),
						switchMap((response) => of({
							integration: 'Sonarr',
							isSuccess: response.isSuccess && response.value?.result === TestConnectionStatus.Success,
						})),
						catchError(() => of({ integration: 'Sonarr', isSuccess: false })),
					),
				);
			}

			// Test Radarr if configured
			if (radarrSettings.isConfigured && radarrSettings.radarrBaseUrl && radarrSettings.radarrApiKey) {
				testObservables.push(
					integrationApi.testConnectionToRadarrEndpoint({
						url: radarrSettings.radarrBaseUrl,
						apiKey: radarrSettings.radarrApiKey,
					}).pipe(
						tap((response) => {
							if (!response.isSuccess || response.value?.result !== TestConnectionStatus.Success) {
								showErrorNotification('Failed to connect to Radarr. Please check your integration settings.');
								state.radarr.testSuccess = false;
								state.radarr.testStatus = response.value?.result ?? TestConnectionStatus.Unknown;
								state.radarr.step = 1;
							} else {
								// If Radarr connection is successful, set step to 3 to show as completed
								state.radarr.step = 3;
								state.radarr.testSuccess = true;
								state.radarr.configuringSuccess = settingsStore.integrationsSettings.radarr.isConfigured;
							}
						}),
						switchMap((response) => of({
							integration: 'Radarr',
							isSuccess: response.isSuccess && response.value?.result === TestConnectionStatus.Success,
						})),
						catchError(() => of({ integration: 'Radarr', isSuccess: false })),
					),
				);
			}

			// If no integrations are configured, return success immediately
			if (testObservables.length === 0) {
				return of({ name: StoreNames.IntegrationStore, isSuccess: true });
			}

			// Test all configured integrations in parallel
			return forkJoin(testObservables).pipe(
				switchMap((results) => {
					const allSuccess = results.every((r) => r.isSuccess);
					return of({ name: StoreNames.IntegrationStore, isSuccess: allSuccess });
				}),
			);
		},

		testConnectionToSonarr() {
			state.sonarr.isTesting = true;
			state.sonarr.testSuccess = null;
			state.sonarr.testStatus = null;
			state.sonarr.error = null;

			return integrationApi.testConnectionToSonarrEndpoint({
				url: settingsStore.integrationsSettings.sonarr.sonarrBaseUrl,
				apiKey: settingsStore.integrationsSettings.sonarr.sonarrApiKey,
			}).pipe(tap((response) => {
				if (response.isSuccess) {
					const status = response.value?.result ?? TestConnectionStatus.Unknown;
					state.sonarr.testStatus = status;

					if (status === TestConnectionStatus.Success) {
						state.sonarr.testSuccess = true;
						state.sonarr.step = 2;
					} else {
						state.sonarr.testSuccess = false;
					}
				} else {
					state.sonarr.testSuccess = false;
					state.sonarr.testStatus = null;
					state.sonarr.error = response;
				}
				state.sonarr.isTesting = false;
			}));
		},

		testConnectionToRadarr() {
			state.radarr.isTesting = true;
			state.radarr.testSuccess = null;
			state.radarr.testStatus = null;
			state.radarr.error = null;

			return integrationApi.testConnectionToRadarrEndpoint({
				url: settingsStore.integrationsSettings.radarr.radarrBaseUrl,
				apiKey: settingsStore.integrationsSettings.radarr.radarrApiKey,
			}).pipe(tap((response) => {
				if (response.isSuccess) {
					const status = response.value?.result ?? TestConnectionStatus.Unknown;
					state.radarr.testStatus = status;

					if (status === TestConnectionStatus.Success) {
						state.radarr.testSuccess = true;
						state.radarr.step = 2;
					} else {
						state.radarr.testSuccess = false;
					}
				} else {
					state.radarr.testSuccess = false;
					state.radarr.testStatus = null;
					state.radarr.error = response;
				}
				state.radarr.isTesting = false;
			}));
		},

		configureSonarrIntegration() {
			state.sonarr.isConfiguring = true;
			state.sonarr.configuringSuccess = null;
			state.sonarr.error = null;

			return integrationApi.configureSonarrIntegrationEndpoint({
				url: settingsStore.integrationsSettings.sonarr.sonarrBaseUrl,
				apiKey: settingsStore.integrationsSettings.sonarr.sonarrApiKey,
			}).pipe(tap((response) => {
				state.sonarr.configuringSuccess = response.isSuccess;
				state.sonarr.isConfiguring = false;
				if (response.isSuccess) {
					// This should overshoot to step 3 to show step 2 as done
					state.sonarr.step = 3;
				} else {
					state.sonarr.error = response;
				}
			}));
		},

		configureRadarrIntegration() {
			state.radarr.isConfiguring = true;
			state.radarr.configuringSuccess = null;
			state.radarr.error = null;

			return integrationApi.configureRadarrIntegrationEndpoint({
				url: settingsStore.integrationsSettings.radarr.radarrBaseUrl,
				apiKey: settingsStore.integrationsSettings.radarr.radarrApiKey,
			}).pipe(tap((response) => {
				state.radarr.configuringSuccess = response.isSuccess;
				state.radarr.isConfiguring = false;
				if (response.isSuccess) {
					// This should overshoot to step 3 to show step 2 as done
					state.radarr.step = 3;
				} else {
					state.radarr.error = response;
				}
			}));
		},

		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	// Getters
	const getters = {
		isRadarrConnectionValid: computed(() => {
			const { radarrBaseUrl, radarrApiKey } = settingsStore.integrationsSettings.radarr;
			return isValidUrl(radarrBaseUrl) && isValidApiKey(radarrApiKey);
		}),
		isSonarrConnectionValid: computed(() => {
			const { sonarrBaseUrl, sonarrApiKey } = settingsStore.integrationsSettings.sonarr;
			return isValidUrl(sonarrBaseUrl) && isValidApiKey(sonarrApiKey);
		}),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useIntegrationStore, import.meta.hot));
}
