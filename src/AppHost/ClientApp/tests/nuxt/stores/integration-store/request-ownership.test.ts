import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { of, Subject } from 'rxjs';
import { baseSetup, subscribeSpyTo } from '@services-test-base';
import { integrationApi } from '@api';
import {
	IntegrationProvisioningState,
	IntegrationType,
	TestConnectionStatus,
	type IntegrationSummary,
	type RadarrIntegrationDTO,
	type SonarrIntegrationDTO,
	type TestConnectionToRadarrEndpointResponse,
	type TestConnectionToSonarrEndpointResponse,
} from '@dto';
import type { ResultDTO } from '@interfaces';
import { useIntegrationStore } from '@store';

type IntegrationDetail = RadarrIntegrationDTO | SonarrIntegrationDTO;
type TestConnectionResult = TestConnectionToRadarrEndpointResponse | TestConnectionToSonarrEndpointResponse;

type FailureKind = 'failure' | 'error';

const success = <T>(value: T): ResultDTO<T> => ({
	isSuccess: true,
	value,
	errors: [],
	successes: [],
	statusCode: 200,
});

const failure = <T>(message = 'Request failed'): ResultDTO<T> => ({
	isSuccess: false,
	errors: [{ message, reasons: [], metadata: {} }],
	successes: [],
	statusCode: 500,
});

function makeDetail(type: IntegrationType, id: string): IntegrationDetail {
	const name = type.toLowerCase();
	return {
		apiKey: `${name}-key`,
		category: name,
		downloadFolderId: 1,
		id,
		lastConnectionTestErrorMessage: null,
		lastConnectionTestHttpStatusCode: null,
		lastConnectionTestStatus: TestConnectionStatus.Unknown,
		lastConnectionTestedAt: null,
		name: type,
		provisioningState: IntegrationProvisioningState.Unconfigured,
		url: `http://${name}`,
	};
}

function makeRadarrDetail(id = 'radarr-id', overrides: Partial<RadarrIntegrationDTO> = {}): RadarrIntegrationDTO {
	return { ...makeDetail(IntegrationType.Radarr, id), ...overrides };
}

function makeSonarrDetail(id = 'sonarr-id', overrides: Partial<SonarrIntegrationDTO> = {}): SonarrIntegrationDTO {
	return { ...makeDetail(IntegrationType.Sonarr, id), ...overrides };
}

function makeSummary(type: IntegrationType, id: string, overrides: Partial<IntegrationSummary> = {}): IntegrationSummary {
	return {
		baseUrl: type === IntegrationType.Radarr ? 'http://radarr' : 'http://sonarr',
		category: type === IntegrationType.Radarr ? 'radarr' : 'sonarr',
		downloadFolderId: 1,
		externalDownloadClientId: null,
		externalIndexerId: null,
		id,
		lastConnectionTestErrorMessage: null,
		lastConnectionTestHttpStatusCode: null,
		lastConnectionTestStatus: TestConnectionStatus.Unknown,
		lastConnectionTestedAt: null,
		name: type,
		provisioningState: IntegrationProvisioningState.Unconfigured,
		type,
		...overrides,
	};
}

type IntegrationStoreDetail = IntegrationDetail & { type: IntegrationType };

interface IntegrationStoreTestState {
	openAdd(type?: IntegrationType): void;
	detail: IntegrationStoreDetail | null;
	draft: {
		type: IntegrationType;
		name: string;
		url: string;
		apiKey: string;
		category: string;
		downloadFolderId: number;
	};
}

interface SubscriptionSpy {
	onComplete(): Promise<void>;
	getValues(): unknown[];
}

function setDraft(store: IntegrationStoreTestState, type: IntegrationType, values: Partial<{
	name: string;
	url: string;
	apiKey: string;
	category: string;
	downloadFolderId: number;
}> = {}): void {
	store.openAdd(type);
	Object.assign(store.draft, {
		name: type,
		url: type === IntegrationType.Radarr ? 'http://radarr' : 'http://sonarr',
		apiKey: type === IntegrationType.Radarr ? 'radarr-key' : 'sonarr-key',
		category: type.toLowerCase(),
		downloadFolderId: 1,
		...values,
	});
}

function setLoadedDetail(store: IntegrationStoreTestState, detail: IntegrationDetail, type: IntegrationType): void {
	store.detail = { ...detail, type };
	Object.assign(store.draft, {
		type,
		name: detail.name,
		url: detail.url,
		apiKey: detail.apiKey,
		category: detail.category,
		downloadFolderId: detail.downloadFolderId,
	});
}

function makeTestResult(status: TestConnectionStatus, testedAt: string): TestConnectionResult {
	return {
		result: status,
		httpStatusCode: status === TestConnectionStatus.Success ? 200 : null,
		errorMessage: status === TestConnectionStatus.Success ? null : 'Connection failed',
		testedAt,
	};
}

function mockEditLoad(type: IntegrationType, response: Subject<ResultDTO<IntegrationDetail>>) {
	const endpoint = type === IntegrationType.Radarr
		? vi.spyOn(integrationApi, 'getRadarrIntegrationEndpoint')
		: vi.spyOn(integrationApi, 'getSonarrIntegrationEndpoint');
	return endpoint.mockReturnValue(response.asObservable());
}

function mockConnectionRequests(type: IntegrationType) {
	const first = new Subject<ResultDTO<TestConnectionResult>>();
	const second = new Subject<ResultDTO<TestConnectionResult>>();
	const endpoint = type === IntegrationType.Radarr
		? vi.spyOn(integrationApi, 'testConnectionToRadarrEndpoint')
		: vi.spyOn(integrationApi, 'testConnectionToSonarrEndpoint');
	endpoint.mockReturnValueOnce(first.asObservable()).mockReturnValue(second.asObservable());
	return { first, second, endpoint };
}

function rejectRequest<T>(response: Subject<ResultDTO<T>>, kind: FailureKind) {
	if (kind === 'failure') {
		response.next(failure<T>('Obsolete request failed'));
		response.complete();
	} else {
		response.error(new Error('Obsolete request threw'));
	}
}

type ChangedTestTarget = 'url' | 'apiKey' | 'id' | 'type';

function beginTargetChange(target: ChangedTestTarget) {
	const store = useIntegrationStore();
	const radarr = mockConnectionRequests(IntegrationType.Radarr);
	const sonarr = mockConnectionRequests(IntegrationType.Sonarr);
	setLoadedDetail(store, makeRadarrDetail('old-id', { apiKey: 'same-key', url: 'http://same-radarr' }), IntegrationType.Radarr);
	const first: SubscriptionSpy = subscribeSpyTo(store.test());
	switch (target) {
		case 'id':
			setLoadedDetail(store, makeRadarrDetail('new-id', { apiKey: 'same-key', url: 'http://same-radarr' }), IntegrationType.Radarr);
			break;
		case 'type':
			setDraft(store, IntegrationType.Sonarr, { url: 'http://sonarr', apiKey: 'same-key' });
			break;
		case 'url':
			store.draft.url = 'http://new-radarr';
			break;
		case 'apiKey':
			store.draft.apiKey = 'new-key';
			break;
	}
	const second: SubscriptionSpy = subscribeSpyTo(store.test());
	const current = target === 'type' ? sonarr.first : radarr.second;
	return { first, second, radarr, sonarr, current };
}

describe('IntegrationStore - request ownership', () => {
	beforeAll(baseSetup);

	beforeEach(() => {
		setActivePinia(createPinia());
		vi.restoreAllMocks();
	});

	test('Should ignore an older edit load when a newer load uses the same integration id', async () => {
		// Arrange
		const store = useIntegrationStore();
		const firstSummary = makeSummary(IntegrationType.Radarr, 'same-id', { name: 'Old Radarr' });
		const secondSummary = makeSummary(IntegrationType.Radarr, 'same-id', { name: 'New Radarr' });
		const firstDetail = makeRadarrDetail('same-id', { name: 'Old Radarr', apiKey: 'old-key' });
		const secondDetail = makeRadarrDetail('same-id', { name: 'New Radarr', apiKey: 'new-key' });
		const firstResponse = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const secondResponse = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		let requestNumber = 0;
		const load = vi.spyOn(integrationApi, 'getRadarrIntegrationEndpoint')
			.mockImplementation(() => (++requestNumber === 1 ? firstResponse : secondResponse).asObservable());
		store.items = [firstSummary, secondSummary];
		store.detail = { ...firstDetail, type: IntegrationType.Radarr };
		Object.assign(store.draft, { type: IntegrationType.Radarr, name: 'Old Radarr', url: firstDetail.url, apiKey: firstDetail.apiKey });

		// Act
		const first = subscribeSpyTo(store.openEdit(firstSummary));
		const second = subscribeSpyTo(store.openEdit(secondSummary));

		// Assert
		expect(load).toHaveBeenNthCalledWith(1, 'same-id');
		expect(load).toHaveBeenNthCalledWith(2, 'same-id');
		expect(store.detail).toBeNull();
		expect(store.isDraftValid).toBe(false);
		expect(store.draft).toMatchObject({ type: IntegrationType.Radarr, name: '', url: '', apiKey: '' });

		firstResponse.next(success(firstDetail));
		firstResponse.complete();
		await first.onComplete();
		expect(store.detail).toBeNull();
		expect(store.isDraftValid).toBe(false);
		expect(store.draft).toMatchObject({ type: IntegrationType.Radarr, name: '', url: '', apiKey: '' });

		secondResponse.next(success(secondDetail));
		secondResponse.complete();
		await second.onComplete();
		expect(second.getFirstValue()).toEqual(success(secondDetail));
		expect(store.detail).toMatchObject({ id: 'same-id', name: 'New Radarr', type: IntegrationType.Radarr });
		expect(store.draft).toMatchObject({ name: 'New Radarr', apiKey: 'new-key' });
	});

	test.each([
		{ staleType: IntegrationType.Radarr, currentType: IntegrationType.Sonarr, kind: 'failure' as FailureKind },
		{ staleType: IntegrationType.Sonarr, currentType: IntegrationType.Radarr, kind: 'error' as FailureKind },
	])('Should ignore a stale edit $kind and preserve the current load state', async ({ staleType, currentType, kind }) => {
		// Arrange
		const store = useIntegrationStore();
		const firstSummary = makeSummary(staleType, 'stale-id');
		const secondSummary = makeSummary(currentType, 'current-id');
		const firstResponse = new Subject<ResultDTO<IntegrationDetail>>();
		const secondResponse = new Subject<ResultDTO<IntegrationDetail>>();
		const secondDetail = currentType === IntegrationType.Radarr ? makeRadarrDetail('current-id') : makeSonarrDetail('current-id');
		const firstLoad = mockEditLoad(staleType, firstResponse);
		const secondLoad = mockEditLoad(currentType, secondResponse);
		store.items = [firstSummary, secondSummary];

		// Act
		const first = subscribeSpyTo(store.openEdit(firstSummary));
		const second = subscribeSpyTo(store.openEdit(secondSummary));
		if (kind === 'failure') {
			firstResponse.next(failure<IntegrationDetail>('Old load failed'));
			firstResponse.complete();
		} else {
			firstResponse.error(new Error('Old load threw'));
		}
		expect(store.isDraftValid).toBe(false);
		await first.onComplete();
		if (kind === 'error') expect(first.getValues()).toEqual([]);

		// Assert
		expect(firstLoad).toHaveBeenCalledWith('stale-id');
		expect(secondLoad).toHaveBeenCalledWith('current-id');
		expect(store.detail).toBeNull();
		expect(store.error).toBeNull();
		expect(store.draft).toMatchObject({ type: currentType, name: '', url: '', apiKey: '' });

		secondResponse.next(success(secondDetail));
		secondResponse.complete();
		await second.onComplete();
		expect(store.error).toBeNull();

		expect(store.detail).toMatchObject({ id: 'current-id', type: currentType });
	});

	test.each(['failure', 'error'] as const)('Should keep an unsuccessful current edit load invalid after a %s', async (kind) => {
		// Arrange
		const store = useIntegrationStore();
		const item = makeSummary(IntegrationType.Radarr, 'current-id');
		const previousDetail = makeRadarrDetail('previous-id', { name: 'Previous Radarr', apiKey: 'previous-key' });
		store.detail = { ...previousDetail, type: IntegrationType.Radarr };
		Object.assign(store.draft, {
			type: IntegrationType.Radarr,
			name: previousDetail.name,
			url: previousDetail.url,
			apiKey: previousDetail.apiKey,
			category: previousDetail.category,
			downloadFolderId: previousDetail.downloadFolderId,
		});
		const response = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const load = vi.spyOn(integrationApi, 'getRadarrIntegrationEndpoint').mockReturnValue(response.asObservable());
		const result = subscribeSpyTo(store.openEdit(item));
		const failed = failure<RadarrIntegrationDTO>('Current load failed');

		// Act
		if (kind === 'failure') {
			response.next(failed);
			response.complete();
		} else {
			response.error(new Error('Current load threw'));
		}
		await result.onComplete();

		// Assert
		expect(load).toHaveBeenCalledWith('current-id');
		expect(store.detail).toBeNull();
		expect(store.isDraftValid).toBe(false);
		expect(store.draft).toMatchObject({ type: IntegrationType.Radarr, name: '', url: '', apiKey: '' });
		if (kind === 'failure') expect(store.error).toEqual(failed);
		else expect(store.error).toBeInstanceOf(Error);
	});

	test.each(['close', 'openAdd', '$reset'] as const)('Should invalidate an edit load when the dialog state is reset by %s', async (operation) => {
		// Arrange
		const store = useIntegrationStore();
		const item = makeSummary(IntegrationType.Radarr, 'stale-id');
		const response = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const load = vi.spyOn(integrationApi, 'getRadarrIntegrationEndpoint').mockReturnValue(response.asObservable());
		const pending = subscribeSpyTo(store.openEdit(item));

		// Act
		switch (operation) {
			case 'close':
				store.close();
				break;
			case 'openAdd':
				store.openAdd(IntegrationType.Sonarr);
				break;
			case '$reset':
				store.$reset();
				break;
		}
		const stateBeforeStaleResponse = {
			detail: store.detail,
			draft: { ...store.draft },
			error: store.error,
			requiresSetupPrompt: store.requiresSetupPrompt,
		};
		response.next(success(makeRadarrDetail('stale-id')));
		response.complete();
		await pending.onComplete();

		// Assert
		expect(load).toHaveBeenCalledWith('stale-id');
		expect(store.detail).toEqual(stateBeforeStaleResponse.detail);
		expect(store.draft).toEqual(stateBeforeStaleResponse.draft);
		expect(store.error).toBe(stateBeforeStaleResponse.error);
		expect(store.requiresSetupPrompt).toBe(stateBeforeStaleResponse.requiresSetupPrompt);
		if (operation === 'openAdd') expect(store.draft.type).toBe(IntegrationType.Sonarr);
		else expect(store.draft).toMatchObject({ type: IntegrationType.Sonarr, name: '', url: '', apiKey: '' });
	});

	test.each(['close', 'openAdd', '$reset'] as const)('Should ignore a stale save after the dialog state is reset by %s', async (operation) => {
		// Arrange
		const store = useIntegrationStore();
		const saveResponse = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const refreshResponse = new Subject<ResultDTO<IntegrationSummary[]>>();
		const save = vi.spyOn(integrationApi, 'createRadarrIntegrationEndpoint').mockReturnValue(saveResponse.asObservable());
		const refresh = vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(refreshResponse.asObservable());
		setDraft(store, IntegrationType.Radarr, { name: 'Old Radarr', url: 'http://old-radarr', apiKey: 'old-key' });
		const result = subscribeSpyTo(store.save());

		// Act
		switch (operation) {
			case 'close':
				store.close();
				break;
			case 'openAdd':
				store.openAdd(IntegrationType.Sonarr);
				break;
			case '$reset':
				store.$reset();
				break;
		}
		const stateBeforeStaleResponse = {
			detail: store.detail,
			draft: { ...store.draft },
			error: store.error,
			saveError: store.saveError,
			requiresSetupPrompt: store.requiresSetupPrompt,
		};
		saveResponse.next(success(makeRadarrDetail('stale-id')));
		saveResponse.complete();
		expect(refresh).toHaveBeenCalledTimes(1);
		refreshResponse.next(success([makeSummary(IntegrationType.Radarr, 'stale-id')]));
		refreshResponse.complete();
		await result.onComplete();

		// Assert
		expect(save).toHaveBeenCalledWith(expect.objectContaining({ url: 'http://old-radarr', apiKey: 'old-key' }));
		expect(result.getValues()).toEqual([]);
		expect(store.items).toEqual([makeSummary(IntegrationType.Radarr, 'stale-id')]);
		expect(store.detail).toEqual(stateBeforeStaleResponse.detail);
		expect(store.draft).toEqual(stateBeforeStaleResponse.draft);
		expect(store.error).toBe(stateBeforeStaleResponse.error);
		expect(store.saveError).toBe(stateBeforeStaleResponse.saveError);
		expect(store.requiresSetupPrompt).toBe(stateBeforeStaleResponse.requiresSetupPrompt);
		expect(store.isSaving).toBe(false);
	});

	test('Should refresh after a stale save but preserve a newer setup and loading state', async () => {
		// Arrange
		const store = useIntegrationStore();
		const firstResponse = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const secondResponse = new Subject<ResultDTO<SonarrIntegrationDTO>>();
		const refreshResponse = new Subject<ResultDTO<IntegrationSummary[]>>();
		const createRadarr = vi.spyOn(integrationApi, 'createRadarrIntegrationEndpoint').mockReturnValue(firstResponse.asObservable());
		const setupSonarr = vi.spyOn(integrationApi, 'setupSonarrIntegrationEndpoint').mockReturnValue(secondResponse.asObservable());
		const refresh = vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(refreshResponse.asObservable());
		setDraft(store, IntegrationType.Radarr, { name: 'Old Radarr', url: 'http://old-radarr', apiKey: 'old-key', category: 'old-radarr' });
		const first = subscribeSpyTo(store.save());
		const currentDetail = makeSonarrDetail('sonarr-current-id', {
			name: 'Current Sonarr',
			url: 'http://current-sonarr',
			apiKey: 'current-key',
			category: 'current-sonarr',
		});
		setLoadedDetail(store, currentDetail, IntegrationType.Sonarr);
		const second = subscribeSpyTo(store.setupIntegration());

		// Act
		firstResponse.next(success(makeRadarrDetail('old-id', { name: 'Old Radarr', url: 'http://old-radarr', apiKey: 'old-key' })));
		firstResponse.complete();
		refreshResponse.next(success([makeSummary(IntegrationType.Radarr, 'old-id')]));
		refreshResponse.complete();
		await first.onComplete();

		// Assert
		expect(createRadarr).toHaveBeenCalledWith({
			name: 'Old Radarr',
			url: 'http://old-radarr',
			apiKey: 'old-key',
			category: 'old-radarr',
			downloadFolderId: 1,
		});
		expect(setupSonarr).toHaveBeenCalledWith('sonarr-current-id');
		expect(refresh).toHaveBeenCalledTimes(1);
		expect(first.getValues()).toEqual([]);
		expect(store.items).toEqual([makeSummary(IntegrationType.Radarr, 'old-id')]);
		expect(store.detail).toMatchObject({ id: 'sonarr-current-id', type: IntegrationType.Sonarr });
		expect(store.draft).toMatchObject({ type: IntegrationType.Sonarr, name: 'Current Sonarr', apiKey: 'current-key' });
		expect(store.requiresSetupPrompt).toBe(false);
		expect(store.error).toBeNull();
		expect(store.saveError).toBeNull();
		expect(store.setupError).toBeNull();
		expect(store.isSettingUp).toBe(true);

		secondResponse.next(failure<SonarrIntegrationDTO>('Current setup failed'));
		secondResponse.complete();
		await second.onComplete();
	});

	test.each([
		{ operation: 'save', kind: 'failure' as FailureKind },
		{ operation: 'save', kind: 'error' as FailureKind },
		{ operation: 'setup', kind: 'failure' as FailureKind },
		{ operation: 'setup', kind: 'error' as FailureKind },
	])('Should ignore a stale $operation $kind without clearing a newer save', async ({ operation, kind }) => {
		// Arrange
		const store = useIntegrationStore();
		const firstResponse = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const secondResponse = new Subject<ResultDTO<SonarrIntegrationDTO>>();
		const oldRequest = operation === 'save'
			? vi.spyOn(integrationApi, 'createRadarrIntegrationEndpoint')
			: vi.spyOn(integrationApi, 'setupRadarrIntegrationEndpoint');
		oldRequest.mockReturnValue(firstResponse.asObservable());
		const createSonarr = vi.spyOn(integrationApi, 'createSonarrIntegrationEndpoint').mockReturnValue(secondResponse.asObservable());
		const refresh = vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(of(success([])));
		const oldDetail = makeRadarrDetail('old-id', { name: 'Old Radarr', url: 'http://old-radarr', apiKey: 'old-key' });
		setDraft(store, IntegrationType.Radarr, { name: oldDetail.name, url: oldDetail.url, apiKey: oldDetail.apiKey, category: oldDetail.category });
		if (operation === 'setup') setLoadedDetail(store, oldDetail, IntegrationType.Radarr);
		const first = subscribeSpyTo(operation === 'save' ? store.save() : store.setupIntegration());
		setDraft(store, IntegrationType.Sonarr, { name: 'Current Sonarr', url: 'http://current-sonarr', apiKey: 'current-key' });
		const second = subscribeSpyTo(store.save());

		// Act
		rejectRequest(firstResponse, kind);
		await first.onComplete();

		// Assert
		if (operation === 'save') expect(oldRequest).toHaveBeenCalledWith(expect.objectContaining({ url: 'http://old-radarr', apiKey: 'old-key' }));
		else expect(oldRequest).toHaveBeenCalledWith('old-id');
		expect(createSonarr).toHaveBeenCalledWith(expect.objectContaining({ url: 'http://current-sonarr', apiKey: 'current-key' }));
		expect(refresh).not.toHaveBeenCalled();
		expect(first.getValues()).toEqual([]);
		expect(store.detail).toBeNull();
		expect(store.draft).toMatchObject({ type: IntegrationType.Sonarr, name: 'Current Sonarr' });
		expect(store.error).toBeNull();
		expect(store.saveError).toBeNull();
		expect(store.setupError).toBeNull();
		expect(store.isSaving).toBe(true);

		secondResponse.next(failure<SonarrIntegrationDTO>('Current save failed'));
		secondResponse.complete();
		await second.onComplete();
	});

	test('Should suppress a save result that becomes stale while its list refresh is pending', async () => {
		// Arrange
		const store = useIntegrationStore();
		const saveResponse = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const refreshResponse = new Subject<ResultDTO<IntegrationSummary[]>>();
		const newerSaveResponse = new Subject<ResultDTO<SonarrIntegrationDTO>>();
		const save = vi.spyOn(integrationApi, 'createRadarrIntegrationEndpoint').mockReturnValue(saveResponse.asObservable());
		const newerSave = vi.spyOn(integrationApi, 'createSonarrIntegrationEndpoint').mockReturnValue(newerSaveResponse.asObservable());
		const refresh = vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(refreshResponse.asObservable());
		setDraft(store, IntegrationType.Radarr, { name: 'Old Radarr', url: 'http://old-radarr', apiKey: 'old-key' });
		const first = subscribeSpyTo(store.save());
		const savedDetail = makeRadarrDetail('old-id', { name: 'Saved Radarr', url: 'http://saved-radarr', apiKey: 'saved-key' });
		saveResponse.next(success(savedDetail));
		saveResponse.complete();
		expect(refresh).toHaveBeenCalledTimes(1);

		// Act
		setDraft(store, IntegrationType.Sonarr, { name: 'Current Sonarr', url: 'http://current-sonarr', apiKey: 'current-key' });
		const second = subscribeSpyTo(store.save());
		refreshResponse.next(success([makeSummary(IntegrationType.Radarr, 'old-id', { name: 'Saved Radarr' })]));
		refreshResponse.complete();
		await first.onComplete();

		// Assert
		expect(save).toHaveBeenCalledWith(expect.objectContaining({ url: 'http://old-radarr', apiKey: 'old-key' }));
		expect(newerSave).toHaveBeenCalledWith(expect.objectContaining({ url: 'http://current-sonarr', apiKey: 'current-key' }));
		expect(first.getValues()).toEqual([]);
		expect(store.items).toEqual([makeSummary(IntegrationType.Radarr, 'old-id', { name: 'Saved Radarr' })]);
		expect(store.detail).toBeNull();
		expect(store.draft).toMatchObject({ type: IntegrationType.Sonarr, name: 'Current Sonarr' });
		expect(store.isSaving).toBe(true);
		expect(store.error).toBeNull();
		expect(store.saveError).toBeNull();

		newerSaveResponse.next(failure<SonarrIntegrationDTO>('Current save failed'));
		newerSaveResponse.complete();
		await second.onComplete();
	});

	test.each(['save', 'setup'] as const)('Should emit a current %s result after applying canonical values and refreshing', async (operation) => {
		// Arrange
		const store = useIntegrationStore();
		const loaded = makeRadarrDetail('radarr-id');
		setLoadedDetail(store, loaded, IntegrationType.Radarr);
		const canonical = makeRadarrDetail('radarr-id', {
			name: 'Canonical Radarr',
			url: 'http://canonical-radarr',
			apiKey: 'canonical-key',
			category: 'canonical-radarr',
			provisioningState: IntegrationProvisioningState.Configured,
		});
		const expectedResult = success(canonical);
		vi.spyOn(integrationApi, 'updateRadarrIntegrationEndpoint').mockReturnValue(of(expectedResult));
		vi.spyOn(integrationApi, 'setupRadarrIntegrationEndpoint').mockReturnValue(of(expectedResult));
		const refresh = vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(of(success([])));

		// Act
		const result = subscribeSpyTo(operation === 'save' ? store.save() : store.setupIntegration());
		await result.onComplete();

		// Assert
		expect(result.getValues()).toEqual([expectedResult]);
		expect(store.detail).toMatchObject({ id: 'radarr-id', name: 'Canonical Radarr' });
		expect(store.draft).toMatchObject({
			name: 'Canonical Radarr',
			url: 'http://canonical-radarr',
			apiKey: 'canonical-key',
			category: 'canonical-radarr',
		});
		expect(refresh).toHaveBeenCalledTimes(1);
		expect(store.isSaving).toBe(false);
		expect(store.isSettingUp).toBe(false);
	});

	test('Should preserve a draft edited after save submission without emitting a close result', async () => {
		// Arrange
		const store = useIntegrationStore();
		const saveResponse = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const refreshResponse = new Subject<ResultDTO<IntegrationSummary[]>>();
		const save = vi.spyOn(integrationApi, 'createRadarrIntegrationEndpoint').mockReturnValue(saveResponse.asObservable());
		vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(refreshResponse.asObservable());
		setDraft(store, IntegrationType.Radarr, {
			name: 'Submitted Radarr',
			url: 'http://submitted-radarr',
			apiKey: 'submitted-key',
			category: 'submitted-radarr',
		});
		const result = subscribeSpyTo(store.save());
		store.draft.name = 'Edited after submit';
		store.draft.url = 'http://edited-after-submit';
		const canonical = makeRadarrDetail('canonical-id', {
			name: 'Canonical Radarr',
			url: 'http://canonical-radarr',
			apiKey: 'canonical-key',
			category: 'canonical-radarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
		});
		const saveResult = success(canonical);

		// Act
		saveResponse.next(saveResult);
		saveResponse.complete();
		refreshResponse.next(success([makeSummary(IntegrationType.Radarr, 'canonical-id', { name: 'Canonical Radarr' })]));
		refreshResponse.complete();
		await result.onComplete();

		// Assert
		expect(save).toHaveBeenCalledWith({
			name: 'Submitted Radarr',
			url: 'http://submitted-radarr',
			apiKey: 'submitted-key',
			category: 'submitted-radarr',
			downloadFolderId: 1,
		});
		expect(result.getValues()).toEqual([]);
		expect(store.detail).toBeNull();
		expect(store.draft).toMatchObject({
			name: 'Edited after submit',
			url: 'http://edited-after-submit',
			apiKey: 'submitted-key',
			category: 'submitted-radarr',
		});
		expect(store.requiresSetupPrompt).toBe(false);
		expect(store.isSaving).toBe(false);
	});

	test('Should refresh after a stale setup but preserve a newer draft and loading state', async () => {
		// Arrange
		const store = useIntegrationStore();
		const setupResponse = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const newerSaveResponse = new Subject<ResultDTO<SonarrIntegrationDTO>>();
		const refreshResponse = new Subject<ResultDTO<IntegrationSummary[]>>();
		const setup = vi.spyOn(integrationApi, 'setupRadarrIntegrationEndpoint').mockReturnValue(setupResponse.asObservable());
		const newerSave = vi.spyOn(integrationApi, 'createSonarrIntegrationEndpoint').mockReturnValue(newerSaveResponse.asObservable());
		const refresh = vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(refreshResponse.asObservable());
		const oldDetail = makeRadarrDetail('old-id');
		setDraft(store, IntegrationType.Radarr, { name: oldDetail.name, url: oldDetail.url, apiKey: oldDetail.apiKey, category: oldDetail.category });
		setLoadedDetail(store, oldDetail, IntegrationType.Radarr);
		const first = subscribeSpyTo(store.setupIntegration());
		setDraft(store, IntegrationType.Sonarr, { name: 'Current Sonarr', url: 'http://current-sonarr', apiKey: 'current-key' });
		const second = subscribeSpyTo(store.save());

		// Act
		setupResponse.next(success(makeRadarrDetail('old-id', { provisioningState: IntegrationProvisioningState.Configured })));
		setupResponse.complete();
		refreshResponse.next(success([makeSummary(IntegrationType.Radarr, 'old-id', { provisioningState: IntegrationProvisioningState.Configured })]));
		refreshResponse.complete();
		await first.onComplete();

		// Assert
		expect(setup).toHaveBeenCalledWith('old-id');
		expect(newerSave).toHaveBeenCalledWith(expect.objectContaining({ url: 'http://current-sonarr', apiKey: 'current-key' }));
		expect(refresh).toHaveBeenCalledTimes(1);
		expect(first.getValues()).toEqual([]);
		expect(store.items).toEqual([makeSummary(IntegrationType.Radarr, 'old-id', { provisioningState: IntegrationProvisioningState.Configured })]);
		expect(store.detail).toBeNull();
		expect(store.draft).toMatchObject({ type: IntegrationType.Sonarr, name: 'Current Sonarr' });
		expect(store.requiresSetupPrompt).toBe(false);
		expect(store.error).toBeNull();
		expect(store.setupError).toBeNull();
		expect(store.saveError).toBeNull();
		expect(store.isSaving).toBe(true);

		newerSaveResponse.next(failure<SonarrIntegrationDTO>('Current save failed'));
		newerSaveResponse.complete();
		await second.onComplete();
	});

	test('Should not emit a close result when the draft changes during the successful-save refresh', async () => {
		// Arrange
		const store = useIntegrationStore();
		setDraft(store, IntegrationType.Radarr);
		const saved = makeRadarrDetail('saved-id');
		vi.spyOn(integrationApi, 'createRadarrIntegrationEndpoint').mockReturnValue(of(success(saved)));
		const refresh = new Subject<ResultDTO<IntegrationSummary[]>>();
		vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(refresh.asObservable());
		const result = subscribeSpyTo(store.save());
		store.draft.name = 'Edited during refresh';

		// Act
		refresh.next(success([makeSummary(IntegrationType.Radarr, 'saved-id')]));
		refresh.complete();
		await result.onComplete();

		// Assert
		expect(result.getValues()).toEqual([]);
		expect(store.detail).toMatchObject({ id: 'saved-id', name: 'Radarr' });
		expect(store.draft.name).toBe('Edited during refresh');
		expect(store.isSaving).toBe(false);
	});

	test('Should preserve a draft edited after setup submission without applying its obsolete result', async () => {
		// Arrange
		const store = useIntegrationStore();
		const setupResponse = new Subject<ResultDTO<RadarrIntegrationDTO>>();
		const refreshResponse = new Subject<ResultDTO<IntegrationSummary[]>>();
		const setup = vi.spyOn(integrationApi, 'setupRadarrIntegrationEndpoint').mockReturnValue(setupResponse.asObservable());
		vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(refreshResponse.asObservable());
		const loaded = makeRadarrDetail('radarr-id', {
			name: 'Loaded Radarr',
			url: 'http://loaded-radarr',
			apiKey: 'loaded-key',
			category: 'loaded-radarr',
		});
		setLoadedDetail(store, loaded, IntegrationType.Radarr);
		const result = subscribeSpyTo(store.setupIntegration());
		store.draft.name = 'Edited after setup';
		store.draft.category = 'edited-category';
		const canonical = makeRadarrDetail('radarr-id', {
			name: 'Provider Radarr',
			url: 'http://provider-radarr',
			apiKey: 'provider-key',
			category: 'provider-radarr',
			provisioningState: IntegrationProvisioningState.Configured,
		});
		const setupResult = success(canonical);

		// Act
		setupResponse.next(setupResult);
		setupResponse.complete();
		refreshResponse.next(success([makeSummary(IntegrationType.Radarr, 'radarr-id', { provisioningState: IntegrationProvisioningState.Configured })]));
		refreshResponse.complete();
		await result.onComplete();

		// Assert
		expect(setup).toHaveBeenCalledWith('radarr-id');
		expect(result.getValues()).toEqual([]);
		expect(store.detail).toMatchObject({ id: 'radarr-id', name: 'Loaded Radarr', url: 'http://loaded-radarr' });
		expect(store.draft).toMatchObject({
			name: 'Edited after setup',
			category: 'edited-category',
			url: 'http://loaded-radarr',
			apiKey: 'loaded-key',
		});
		expect(store.requiresSetupPrompt).toBe(false);
		expect(store.isSettingUp).toBe(false);
		expect(store.error).toBeNull();
	});

	test.each([
		{ type: IntegrationType.Radarr, kind: 'failure' as FailureKind },
		{ type: IntegrationType.Radarr, kind: 'error' as FailureKind },
		{ type: IntegrationType.Sonarr, kind: 'failure' as FailureKind },
		{ type: IntegrationType.Sonarr, kind: 'error' as FailureKind },
	])('Should keep a newer test loading after a stale $kind for $type and preserve its endpoint target', async ({ type, kind }) => {
		// Arrange
		const store = useIntegrationStore();
		const requests = mockConnectionRequests(type);
		const host = type.toLowerCase();
		setDraft(store, type, { url: `http://${host}-old`, apiKey: 'old-key' });
		const first = subscribeSpyTo(store.test());
		store.draft.url = `http://${host}-new`;
		store.draft.apiKey = 'new-key';
		const second = subscribeSpyTo(store.test());

		// Act
		rejectRequest(requests.first, kind);
		await first.onComplete();

		// Assert
		expect(requests.endpoint).toHaveBeenNthCalledWith(1, { url: `http://${host}-old`, apiKey: 'old-key' });
		expect(requests.endpoint).toHaveBeenNthCalledWith(2, { url: `http://${host}-new`, apiKey: 'new-key' });
		expect(store.error).toBeNull();
		expect(store.testResult).toBeNull();
		expect(store.isTesting).toBe(true);
		expect(first.getValues()).toEqual([]);

		const currentResult = makeTestResult(TestConnectionStatus.Success, '2026-10-20T12:00:00Z');
		requests.second.next(success(currentResult));
		requests.second.complete();
		await second.onComplete();
		expect(store.testResult).toEqual(currentResult);
		expect(store.error).toBeNull();
		expect(store.isTesting).toBe(false);
	});

	test.each([
		{ target: 'url', query: { url: 'http://new-radarr', apiKey: 'same-key', integrationId: 'old-id' } },
		{ target: 'apiKey', query: { url: 'http://same-radarr', apiKey: 'new-key', integrationId: 'old-id' } },
		{ target: 'id', query: { url: 'http://same-radarr', apiKey: 'same-key', integrationId: 'new-id' } },
		{ target: 'type', query: { url: 'http://sonarr', apiKey: 'same-key' } },
	] as const)('Should reject an obsolete successful test when its $target target changes', async ({ target, query }) => {
		// Arrange
		const store = useIntegrationStore();
		const pending = beginTargetChange(target);
		const oldResult = makeTestResult(TestConnectionStatus.Success, '2026-10-20T12:00:00Z');

		// Act
		pending.radarr.first.next(success(oldResult));
		pending.radarr.first.complete();
		await pending.first.onComplete();

		// Assert stale result does not become current
		expect(store.testResult).toBeNull();
		expect(pending.first.getValues()).toEqual([]);
		expect(pending.radarr.endpoint).toHaveBeenNthCalledWith(1, { url: 'http://same-radarr', apiKey: 'same-key', integrationId: 'old-id' });

		const currentResult = makeTestResult(TestConnectionStatus.Success, '2026-10-20T12:01:00Z');
		pending.current.next(success(currentResult));
		pending.current.complete();
		await pending.second.onComplete();
		const currentEndpoint = target === 'type' ? pending.sonarr.endpoint : pending.radarr.endpoint;
		expect(currentEndpoint).toHaveBeenLastCalledWith(query);
		expect(store.testResult).toEqual(currentResult);
		expect(store.isTesting).toBe(false);
	});
});
