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
	type IntegrationSummary,
	IntegrationType,
	type RadarrIntegrationDTO,
	type SonarrIntegrationDTO,
	type TestConnectionToRadarrEndpointResponse,
	type TestConnectionToSonarrEndpointResponse,
} from '@dto';
import { type ISetupResult, type ResultDTO, StoreNames } from '@interfaces';

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
			const testResult = state.testResult;
			return integrationApi.getIntegrationsEndpoint().pipe(
				tap((result) => {
					if (result.isSuccess) {
						state.items = result.value ?? [];
						if (testResult) updateSummaryConnectionStatus(testResult);
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
			let request: Observable<ResultDTO<RadarrIntegrationDTO | SonarrIntegrationDTO>>;
			switch (item.type) {
				case IntegrationType.Radarr:
					request = integrationApi.getRadarrIntegrationEndpoint(item.id);
					break;
				case IntegrationType.Sonarr:
					request = integrationApi.getSonarrIntegrationEndpoint(item.id);
					break;
				default:
					throw new Error(`Unsupported integration type: ${String(item.type)}`);
			}
			return request.pipe(
				tap((result) => {
					if (result.isSuccess && result.value) setDetail(item.type, result.value);
					else state.error = result;
				}),
				catchError(handleError),
			);
		},
		test() {
			const query = {
				apiKey: state.draft.apiKey,
				url: state.draft.url,
				...(state.detail ? { integrationId: state.detail.id } : {}),
			};
			let request: Observable<ResultDTO<TestConnectionResult>>;
			switch (state.draft.type) {
				case IntegrationType.Radarr:
					request = integrationApi.testConnectionToRadarrEndpoint(query);
					break;
				case IntegrationType.Sonarr:
					request = integrationApi.testConnectionToSonarrEndpoint(query);
					break;
				default:
					throw new Error(`Unsupported integration type: ${String(state.draft.type)}`);
			}
			state.isTesting = true;
			state.testResult = null;
			state.error = null;
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
			let request: Observable<ResultDTO<RadarrIntegrationDTO | SonarrIntegrationDTO>>;
			switch (state.draft.type) {
				case IntegrationType.Radarr:
					request = saveRadarr();
					break;
				case IntegrationType.Sonarr:
					request = saveSonarr();
					break;
				default:
					throw new Error(`Unsupported integration type: ${String(state.draft.type)}`);
			}
			state.isSaving = true;
			state.error = null;
			return request.pipe(
				switchMap((result) => result.isSuccess ? actions.refresh().pipe(map(() => result)) : of(result)),
				catchError(handleError),
				finalize(() => (state.isSaving = false)),
			);
		},
		setupIntegration() {
			const detail = state.detail;
			if (!detail) return of(null);
			let request: Observable<ResultDTO<RadarrIntegrationDTO | SonarrIntegrationDTO>>;
			switch (detail.type) {
				case IntegrationType.Radarr:
					request = integrationApi.setupRadarrIntegrationEndpoint(detail.id);
					break;
				case IntegrationType.Sonarr:
					request = integrationApi.setupSonarrIntegrationEndpoint(detail.id);
					break;
				default:
					throw new Error(`Unsupported integration type: ${String(detail.type)}`);
			}
			state.isSettingUp = true;
			state.error = null;
			state.testResult = null;
			return request.pipe(
				tap((result) => {
					if (result.isSuccess && result.value) {
						setDetail(state.detail!.type, result.value);
						state.requiresSetupPrompt = false;
					}
				}),
				switchMap((result) => result.isSuccess ? actions.refresh().pipe(map(() => result)) : of(result)),
				catchError((error) => {
					Log.error('Integration setup request failed', error);
					return of(null);
				}),
				finalize(() => (state.isSettingUp = false)),
			);
		},
		delete() {
			const detail = state.detail;
			if (!detail) return of(null);
			let request: Observable<unknown>;
			switch (detail.type) {
				case IntegrationType.Radarr:
					request = integrationApi.deleteRadarrIntegrationEndpoint(detail.id, { Force: true });
					break;
				case IntegrationType.Sonarr:
					request = integrationApi.deleteSonarrIntegrationEndpoint(detail.id, { Force: true });
					break;
				default:
					throw new Error(`Unsupported integration type: ${String(detail.type)}`);
			}
			state.isDeleting = true;
			state.error = null;
			return (request as Observable<ResultDTO>).pipe(
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
