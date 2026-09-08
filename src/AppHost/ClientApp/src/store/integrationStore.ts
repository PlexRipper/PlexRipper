import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { computed, reactive, toRefs } from 'vue';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';
import { cloneDeep } from 'lodash-es';
import { integrationApi } from '@api';
import {
	IntegrationProvisioningState,
	IntegrationType,
	type TestConnectionToRadarrEndpointResponse,
	type TestConnectionToSonarrEndpointResponse,
	type IntegrationSummary,
	type RadarrIntegrationDTO,
	type SonarrIntegrationDTO,
} from '@dto';
import { StoreNames, type ISetupResult } from '@interfaces';

interface IIntegrationDraft {
	type: IntegrationType;
	name: string;
	url: string;
	apiKey: string;
	category: string;
	downloadFolderId: number;
}

type IntegrationDetail = (RadarrIntegrationDTO | SonarrIntegrationDTO) & { type: IntegrationType };
type TestConnectionResult = TestConnectionToRadarrEndpointResponse | TestConnectionToSonarrEndpointResponse;

interface IIntegrationStoreState {
	items: IntegrationSummary[];
	detail: IntegrationDetail | null;
	draft: IIntegrationDraft;
	isTesting: boolean;
	isSaving: boolean;
	isSettingUp: boolean;
	isDeleting: boolean;
	testResult: TestConnectionResult | null;
	error: unknown;
	requiresSetupPrompt: boolean;
}

function emptyDraft(type = IntegrationType.Sonarr): IIntegrationDraft {
	return { type, name: '', url: '', apiKey: '', category: `Reaparr ${type}`, downloadFolderId: 1 };
}

export const useIntegrationStore = defineStore(StoreNames.IntegrationStore, () => {
	const defaultState: IIntegrationStoreState = {
		items: [],
		detail: null,
		draft: emptyDraft(),
		isTesting: false,
		isSaving: false,
		isSettingUp: false,
		isDeleting: false,
		testResult: null,
		error: null,
		requiresSetupPrompt: false,
	};

	const state = reactive<IIntegrationStoreState>(cloneDeep(defaultState));

	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.refresh().pipe(
				map(() => ({ name: StoreNames.IntegrationStore, isSuccess: true })),
				catchError((error) => {
					Log.error('Failed to setup integration store', error);
					return of({ name: StoreNames.IntegrationStore, isSuccess: false });
				}),
			);
		},
		refresh() {
			return integrationApi.getIntegrationsEndpoint().pipe(
				tap((result) => {
					if (result.isSuccess) {
						state.items = result.value ?? [];
						if (state.testResult) updateSummaryConnectionStatus(state.testResult);
					}
				}),
			);
		},
		openAdd(type = IntegrationType.Sonarr): void {
			state.detail = null;
			state.draft = emptyDraft(type);
			resetOperationState();
		},
		openEdit(item: IntegrationSummary) {
			resetOperationState();
			const request = item.type === IntegrationType.Radarr
				? integrationApi.getRadarrIntegrationEndpoint(item.id)
				: integrationApi.getSonarrIntegrationEndpoint(item.id);
			return request.pipe(
				tap((result) => {
					if (result.isSuccess && result.value) setDetail(item.type, result.value);
					else state.error = result;
				}),
				catchError(handleError),
			);
		},
		test() {
			state.isTesting = true;
			state.testResult = null;
			state.error = null;
			const query = {
				apiKey: state.draft.apiKey,
				url: state.draft.url,
				...(state.detail ? { integrationId: state.detail.id } : {}),
			};
			const request = state.draft.type === IntegrationType.Radarr
				? integrationApi.testConnectionToRadarrEndpoint(query)
				: integrationApi.testConnectionToSonarrEndpoint(query);
			return request.pipe(
				tap((result) => {
					if (result.value) {
						state.testResult = result.value;
						updateSummaryConnectionStatus(result.value);
					} else state.error = result;
				}),
				catchError(handleError),
				finalize(() => (state.isTesting = false)),
			);
		},
		save() {
			state.isSaving = true;
			state.error = null;
			return (state.draft.type === IntegrationType.Radarr ? saveRadarr() : saveSonarr()).pipe(
				switchMap((result) => result.isSuccess ? actions.refresh().pipe(map(() => result)) : of(result)),
				catchError(handleError),
				finalize(() => (state.isSaving = false)),
			);
		},
		setupIntegration() {
			if (!state.detail) return of(null);
			state.isSettingUp = true;
			state.error = null;
			const request = state.detail.type === IntegrationType.Radarr
				? integrationApi.setupRadarrIntegrationEndpoint(state.detail.id)
				: integrationApi.setupSonarrIntegrationEndpoint(state.detail.id);
			return request.pipe(
				tap((result) => {
					if (result.isSuccess && result.value) {
						setDetail(state.detail!.type, result.value);
						state.requiresSetupPrompt = false;
					} else state.error = result;
				}),
				switchMap((result) => result.isSuccess ? actions.refresh().pipe(map(() => result)) : of(result)),
				catchError(handleError),
				finalize(() => (state.isSettingUp = false)),
			);
		},
		delete(force = false) {
			if (!state.detail) return of(null);
			state.isDeleting = true;
			state.error = null;
			const request: Observable<{ isSuccess: boolean }> = (state.detail.type === IntegrationType.Radarr
				? integrationApi.deleteRadarrIntegrationEndpoint(state.detail.id, { Force: force })
				: integrationApi.deleteSonarrIntegrationEndpoint(state.detail.id, { Force: force })) as Observable<{
				isSuccess: boolean;
			}>;
			return request.pipe(
				switchMap((result) => {
					if (!result.isSuccess) {
						state.error = result;
						return of(result);
					}
					state.detail = null;
					state.requiresSetupPrompt = false;
					return actions.refresh().pipe(map(() => result));
				}),
				catchError(handleError),
				finalize(() => (state.isDeleting = false)),
			);
		},
		close(): void {
			state.detail = null;
			state.draft = emptyDraft();
			resetOperationState();
		},
		$reset(): void {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	function resetOperationState(): void {
		state.error = null;
		state.testResult = null;
		state.requiresSetupPrompt = false;
	}

	function updateSummaryConnectionStatus(result: TestConnectionResult): void {
		if (!state.detail) return;

		const index = state.items.findIndex((item) => item.id === state.detail!.id);
		state.detail = {
			...state.detail,
			lastConnectionTestStatus: result.result,
			lastConnectionTestHttpStatusCode: result.httpStatusCode,
			lastConnectionTestErrorMessage: result.errorMessage,
			lastConnectionTestedAt: result.testedAt,
		};
		if (index === -1) return;

		const integration = state.items[index];
		if (!integration) return;
		state.items.splice(index, 1, {
			...integration,
			lastConnectionTestStatus: result.result,
			lastConnectionTestHttpStatusCode: result.httpStatusCode,
			lastConnectionTestErrorMessage: result.errorMessage,
			lastConnectionTestedAt: result.testedAt,
		});
	}

	function setDetail(type: IntegrationType, detail: RadarrIntegrationDTO | SonarrIntegrationDTO): void {
		state.detail = { ...detail, type };
		state.draft = {
			type,
			name: detail.name,
			url: detail.url,
			apiKey: detail.apiKey,
			category: detail.category,
			downloadFolderId: detail.downloadFolderId,
		};
	}

	function saveRadarr() {
		const request = toRequest();
		const observable = state.detail
			? integrationApi.updateRadarrIntegrationEndpoint(state.detail.id, request)
			: integrationApi.createRadarrIntegrationEndpoint(request);
		return observable.pipe(tap((result) => handleSaveResult(IntegrationType.Radarr, result)));
	}

	function saveSonarr() {
		const request = toRequest();
		const observable = state.detail
			? integrationApi.updateSonarrIntegrationEndpoint(state.detail.id, request)
			: integrationApi.createSonarrIntegrationEndpoint(request);
		return observable.pipe(tap((result) => handleSaveResult(IntegrationType.Sonarr, result)));
	}

	function toRequest() {
		return {
			name: state.draft.name,
			url: state.draft.url,
			apiKey: state.draft.apiKey,
			category: state.draft.category,
			downloadFolderId: state.draft.downloadFolderId,
		};
	}

	function handleSaveResult(type: IntegrationType, result: {
		isSuccess: boolean;
		value?: RadarrIntegrationDTO | SonarrIntegrationDTO | null;
	}): void {
		if (result.isSuccess && result.value) {
			setDetail(type, result.value);
			state.requiresSetupPrompt = result.value.provisioningState !== IntegrationProvisioningState.Configured;
		} else state.error = result;
	}

	function handleError(error: unknown) {
		state.error = error;
		Log.error('Integration request failed', error);
		return of(null);
	}

	const getters = {
		isDraftValid: computed(() => Boolean(
			state.draft.name.trim()
			&& state.draft.url.trim()
			&& state.draft.apiKey.trim()
			&& state.draft.category.trim()
			&& state.draft.downloadFolderId > 0,
		)),
	};

	return { ...toRefs(state), ...actions, ...getters };
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useIntegrationStore, import.meta.hot));
}
