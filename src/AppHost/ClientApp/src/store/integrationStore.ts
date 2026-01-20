import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, toRefs } from 'vue';
import { switchMap, tap, catchError } from 'rxjs/operators';
import { type Observable, of, forkJoin } from 'rxjs';
import { useSubscription } from '@vueuse/rxjs';
import { cloneDeep } from 'lodash-es';
import { type BaseResultDTO, TestConnectionStatus } from '@dto';
import { integrationApi } from '@api';
import { useSettingsStore } from '@store';
import type { ISetupResult } from '@interfaces';

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

export const useIntegrationStore = defineStore('IntegrationStore', () => {
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
				return of({ name: 'useIntegrationStore', isSuccess: true });
			}

			// Test all configured integrations in parallel
			return forkJoin(testObservables).pipe(
				switchMap((results) => {
					const allSuccess = results.every((r) => r.isSuccess);
					return of({ name: 'useIntegrationStore', isSuccess: allSuccess });
				}),
			);
		},

		testConnection(integrationType: 'sonarr' | 'radarr', url: string, apiKey: string) {
			if (url === '' || apiKey === '') {
				return;
			}

			const integrationState = state[integrationType];

			integrationState.isTesting = true;
			integrationState.testSuccess = null;
			integrationState.testStatus = null;
			integrationState.error = null;

			const testEndpoint = integrationType === 'sonarr'
				? integrationApi.testConnectionToSonarrEndpoint
				: integrationApi.testConnectionToRadarrEndpoint;

			useSubscription(testEndpoint({
				url,
				apiKey,
			}).subscribe((response) => {
				if (response.isSuccess) {
					const status = response.value?.result ?? TestConnectionStatus.Unknown;
					integrationState.testStatus = status;

					if (status === TestConnectionStatus.Success) {
						integrationState.testSuccess = true;
						integrationState.step = 2;
					} else {
						integrationState.testSuccess = false;
					}
				} else {
					integrationState.testSuccess = false;
					integrationState.testStatus = null;
					integrationState.error = response;
				}
				integrationState.isTesting = false;
			}));
		},

		configureIntegration(integrationType: 'sonarr' | 'radarr', url: string, apiKey: string) {
			const integrationState = state[integrationType];

			integrationState.isConfiguring = true;
			integrationState.configuringSuccess = null;
			integrationState.error = null;

			const configureEndpoint = integrationType === 'sonarr'
				? integrationApi.configureSonarrIntegrationEndpoint
				: integrationApi.configureRadarrIntegrationEndpoint;

			useSubscription(configureEndpoint({
				url,
				apiKey,
			}).pipe(switchMap((response) => {
				// Wait for refreshSettings to complete before processing the response
				// This ensures isConfigured is updated before the component checks it
				return settingsStore.refreshSettings().pipe(switchMap(() => of(response)));
			})).subscribe((response) => {
				integrationState.configuringSuccess = response.isSuccess;
				integrationState.isConfiguring = false;
				if (response.isSuccess) {
					// This should overshoot to step 3 to show step 2 as done
					integrationState.step = 3;
				} else {
					integrationState.error = response;
				}
			}));
		},

		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	return {
		...toRefs(state),
		...actions,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useIntegrationStore, import.meta.hot));
}
