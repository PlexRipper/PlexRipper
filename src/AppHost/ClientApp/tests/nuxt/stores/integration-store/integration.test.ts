import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { Subject, of } from 'rxjs';
import { baseSetup, subscribeSpyTo } from '@services-test-base';
import { integrationApi } from '@api';
import { IntegrationProvisioningState, IntegrationType, TestConnectionStatus, type IntegrationSummary } from '@dto';
import type { ResultDTO } from '@interfaces';
import { useIntegrationStore } from '@store';

const success = <T>(value: T) => of({ isSuccess: true, value, errors: [], successes: [], statusCode: 200 });

describe('IntegrationStore', () => {
	beforeAll(baseSetup);

	beforeEach(() => {
		setActivePinia(createPinia());
		vi.restoreAllMocks();
	});

	test('Should open a typed draft when adding an integration', () => {
		// Arrange
		const store = useIntegrationStore();

		// Act
		store.openAdd(IntegrationType.Radarr);

		// Assert
		expect(store.draft).toMatchObject({ type: IntegrationType.Radarr, name: '', url: '', apiKey: '' });
	});

	test('Should choose an unused default category when adding another integration', () => {
		// Arrange
		const store = useIntegrationStore();
		store.items = [{
			baseUrl: 'http://sonarr',
			category: 'reaparr-sonarr',
			downloadFolderId: 1,
			externalDownloadClientId: null,
			externalIndexerId: null,
			id: 'id',
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
			name: 'Sonarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			type: IntegrationType.Sonarr,
		}];

		// Act
		store.openAdd(IntegrationType.Sonarr);

		// Assert
		expect(store.draft.category).toBe('reaparr-sonarr-2');
	});

	test('Should reject a duplicate category or base URL from the same integration type', () => {
		// Arrange
		const store = useIntegrationStore();
		store.items = [{
			baseUrl: 'http://sonarr',
			category: 'reaparr-sonarr',
			downloadFolderId: 1,
			externalDownloadClientId: null,
			externalIndexerId: null,
			id: 'id',
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
			name: 'Sonarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			type: IntegrationType.Sonarr,
		}];
		store.openAdd(IntegrationType.Sonarr);
		Object.assign(store.draft, { name: 'New Sonarr', apiKey: 'key', category: 'new-category' });

		// Act
		store.draft.url = 'http://sonarr';

		// Assert
		expect(store.isDraftValid).toBe(false);
		store.draft.url = 'http://new-sonarr';
		store.draft.category = 'reaparr-sonarr';
		expect(store.isDraftValid).toBe(false);
	});

	test('Should create a Sonarr integration through the typed endpoint', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.openAdd(IntegrationType.Sonarr);
		Object.assign(store.draft, { name: 'Sonarr', url: 'http://sonarr', apiKey: 'key', category: 'sonarr' });
		const detail = {
			id: 'id', name: 'Sonarr', url: 'http://sonarr', apiKey: 'key', category: 'sonarr', downloadFolderId: 1,
			provisioningState: IntegrationProvisioningState.Unconfigured,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
		};
		const create = vi.spyOn(integrationApi, 'createSonarrIntegrationEndpoint').mockReturnValue(success(detail));
		vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(success([]));

		// Act
		const result = subscribeSpyTo(store.save());
		await result.onComplete();

		// Assert
		expect(result.getLastValue()).toEqual(expect.objectContaining({ isSuccess: true }));
		expect(create).toHaveBeenCalledWith({
			name: 'Sonarr',
			url: 'http://sonarr',
			apiKey: 'key',
			category: 'sonarr',
			downloadFolderId: 1,
		});
		expect(store.detail?.type).toBe(IntegrationType.Sonarr);
		expect(store.requiresSetupPrompt).toBe(true);
	});

	test('Should preserve save failures separately from connection results', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.openAdd(IntegrationType.Sonarr);
		Object.assign(store.draft, { name: 'Sonarr', url: 'http://sonarr', apiKey: 'key', category: 'sonarr' });
		const failedSave = { isSuccess: false, errors: [{ message: 'Save failed', reasons: [], metadata: {} }], successes: [], statusCode: 500 };
		vi.spyOn(integrationApi, 'createSonarrIntegrationEndpoint').mockReturnValue(of(failedSave));

		// Act
		const result = subscribeSpyTo(store.save());
		await result.onComplete();

		// Assert
		expect(result.getLastValue()).toEqual(failedSave);
		expect(store.saveError).toEqual(failedSave);
		expect(store.error).toEqual(failedSave);
	});

	test('Should test a Radarr draft through the typed endpoint', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.openAdd(IntegrationType.Radarr);
		Object.assign(store.draft, { url: 'http://radarr', apiKey: 'key' });
		const testResult = {
			result: TestConnectionStatus.Success,
			httpStatusCode: 200,
			errorMessage: null,
			testedAt: '2026-09-07T12:00:00Z',
		};
		const testConnection = vi.spyOn(integrationApi, 'testConnectionToRadarrEndpoint')
			.mockReturnValue(success(testResult));

		// Act
		await subscribeSpyTo(store.test()).onComplete();

		// Assert
		expect(testConnection).toHaveBeenCalledWith({ url: 'http://radarr', apiKey: 'key' });
		expect(store.testResult).toEqual(testResult);
	});

	test('Should update an existing integration summary after a failed connection test', async () => {
		// Arrange
		const store = useIntegrationStore();
		const testResult = {
			result: TestConnectionStatus.ConnectionFailed,
			httpStatusCode: null,
			errorMessage: 'Connection refused',
			testedAt: '2026-09-07T12:00:00Z',
		};
		store.items = [{
			baseUrl: 'http://radarr',
			category: 'radarr',
			downloadFolderId: 1,
			externalDownloadClientId: null,
			externalIndexerId: null,
			id: 'id',
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Success,
			lastConnectionTestedAt: null,
			name: 'Radarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			type: IntegrationType.Radarr,
		}];
		store.detail = {
			apiKey: 'key',
			category: 'radarr',
			downloadFolderId: 1,
			id: 'id',
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Success,
			lastConnectionTestedAt: null,
			name: 'Radarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			url: 'http://radarr',
			type: IntegrationType.Radarr,
		};
		Object.assign(store.draft, { type: IntegrationType.Radarr, url: 'http://radarr', apiKey: 'key' });
		const testConnection = vi.spyOn(integrationApi, 'testConnectionToRadarrEndpoint').mockReturnValue(success(testResult));

		// Act
		await subscribeSpyTo(store.test()).onComplete();

		// Assert
		expect(testConnection).toHaveBeenCalledWith({ url: 'http://radarr', apiKey: 'key', integrationId: 'id' });
		expect(store.items[0]!.lastConnectionTestStatus).toBe(TestConnectionStatus.ConnectionFailed);
		expect(store.items[0]!.lastConnectionTestErrorMessage).toBe('Connection refused');
		expect(store.items[0]!.lastConnectionTestHttpStatusCode).toBeNull();
		expect(store.isTesting).toBe(false);
	});

	test('Should send current Sonarr draft credentials when testing an existing integration', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.detail = {
			apiKey: 'draft-key',
			category: 'sonarr',
			downloadFolderId: 1,
			id: 'sonarr-id',
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
			name: 'Sonarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			url: 'http://sonarr-draft',
			type: IntegrationType.Sonarr,
		};
		Object.assign(store.draft, { type: IntegrationType.Sonarr, url: 'http://sonarr-draft', apiKey: 'draft-key' });
		const testResult = {
			result: TestConnectionStatus.Success,
			httpStatusCode: 200,
			errorMessage: null,
			testedAt: '2026-09-07T12:00:00Z',
		};
		const testConnection = vi.spyOn(integrationApi, 'testConnectionToSonarrEndpoint').mockReturnValue(success(testResult));

		// Act
		await subscribeSpyTo(store.test()).onComplete();

		// Assert
		expect(testConnection).toHaveBeenCalledWith({ url: 'http://sonarr-draft', apiKey: 'draft-key', integrationId: 'sonarr-id' });
		expect(store.testResult).toEqual(testResult);
		expect(store.isTesting).toBe(false);
	});

	test('Should clear connection alerts when setup starts', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.detail = {
			apiKey: 'key',
			category: 'radarr',
			downloadFolderId: 1,
			id: 'id',
			lastConnectionTestErrorMessage: 'Connection refused',
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.ConnectionFailed,
			lastConnectionTestedAt: '2026-09-07T12:00:00Z',
			name: 'Radarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			url: 'http://radarr',
			type: IntegrationType.Radarr,
		};
		store.testResult = {
			result: TestConnectionStatus.ConnectionFailed,
			httpStatusCode: null,
			errorMessage: 'Connection refused',
			testedAt: '2026-09-07T12:00:00Z',
		};
		store.error = 'Connection failed';
		const detail = {
			id: 'id', name: 'Radarr', url: 'http://radarr', apiKey: 'key', category: 'radarr', downloadFolderId: 1,
			provisioningState: IntegrationProvisioningState.Configured,
			lastConnectionTestStatus: TestConnectionStatus.Success,
		};
		const setup = vi.spyOn(integrationApi, 'setupRadarrIntegrationEndpoint').mockReturnValue(success(detail));
		vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(success([]));

		// Act
		await subscribeSpyTo(store.setupIntegration()).onComplete();

		// Assert
		expect(setup).toHaveBeenCalledWith('id');
		expect(store.testResult).toBeNull();
		expect(store.error).toBeNull();
		expect(store.isSettingUp).toBe(false);
	});

	test('Should not expose setup failure as a connection alert', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.detail = {
			apiKey: 'key',
			category: 'radarr',
			downloadFolderId: 1,
			id: 'id',
			lastConnectionTestErrorMessage: 'Connection refused',
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.ConnectionFailed,
			lastConnectionTestedAt: '2026-09-07T12:00:00Z',
			name: 'Radarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			url: 'http://radarr',
			type: IntegrationType.Radarr,
		};
		store.testResult = {
			result: TestConnectionStatus.ConnectionFailed,
			httpStatusCode: null,
			errorMessage: 'Connection refused',
			testedAt: '2026-09-07T12:00:00Z',
		};
		store.error = 'Connection failed';
		const failedSetup = { isSuccess: false, errors: [{ message: 'Setup failed', reasons: [], metadata: {} }], successes: [], statusCode: 500 };
		const setup = vi.spyOn(integrationApi, 'setupRadarrIntegrationEndpoint').mockReturnValue(of(failedSetup));

		// Act
		const result = subscribeSpyTo(store.setupIntegration());
		await result.onComplete();

		// Assert
		expect(setup).toHaveBeenCalledWith('id');
		expect(result.getLastValue()).toEqual(failedSetup);
		expect(store.error).toBeNull();
		expect(store.setupError).toEqual(failedSetup);
		expect(store.testResult).toBeNull();
	});

	test('Should preserve setup failures for the setup dialog', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.detail = {
			apiKey: 'key',
			category: 'radarr',
			downloadFolderId: 1,
			id: 'id',
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
			name: 'Radarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			url: 'http://radarr',
			type: IntegrationType.Radarr,
		};
		const failedSetup = { isSuccess: false, errors: [{ message: 'Setup failed', reasons: [], metadata: {} }], successes: [], statusCode: 500 };
		vi.spyOn(integrationApi, 'setupRadarrIntegrationEndpoint').mockReturnValue(of(failedSetup));

		// Act
		const result = subscribeSpyTo(store.setupIntegration());
		await result.onComplete();

		// Assert
		expect(result.getLastValue()).toEqual(failedSetup);
		expect(store.setupError).toEqual(failedSetup);
		expect(store.error).toBeNull();
		expect(store.isSettingUp).toBe(false);
	});

	test('Should not report a stale connection test as current after credentials change', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.openAdd(IntegrationType.Radarr);
		Object.assign(store.draft, { url: 'http://radarr', apiKey: 'old-key' });
		const testResult = {
			result: TestConnectionStatus.Success,
			httpStatusCode: 200,
			errorMessage: null,
			testedAt: '2026-09-07T12:00:00Z',
		};
		const pending = new Subject<ResultDTO<typeof testResult>>();
		vi.spyOn(integrationApi, 'testConnectionToRadarrEndpoint').mockReturnValue(pending.asObservable());

		// Act
		const result = subscribeSpyTo(store.test());
		store.draft.apiKey = 'new-key';
		pending.next({ isSuccess: true, value: testResult, errors: [], successes: [], statusCode: 200 });
		pending.complete();
		await result.onComplete();

		// Assert
		expect(store.testResult).toBeNull();
		expect(store.isSetupDisabled).toBe(true);
	});

	test('Should update an existing integration summary after a successful connection test', async () => {
		// Arrange
		const store = useIntegrationStore();
		const testResult = {
			result: TestConnectionStatus.Success,
			httpStatusCode: 200,
			errorMessage: null,
			testedAt: '2026-09-07T12:00:00Z',
		};

		store.items = [{
			baseUrl: 'http://radarr',
			category: 'radarr',
			downloadFolderId: 1,
			externalDownloadClientId: null,
			externalIndexerId: null,
			id: 'id',
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
			name: 'Radarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			type: IntegrationType.Radarr,
		}];
		store.detail = {
			apiKey: 'key',
			category: 'radarr',
			downloadFolderId: 1,
			id: 'id',
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
			name: 'Radarr',
			provisioningState: IntegrationProvisioningState.Unconfigured,
			url: 'http://radarr',
			type: IntegrationType.Radarr,
		};
		Object.assign(store.draft, { type: IntegrationType.Radarr, url: 'http://radarr', apiKey: 'key' });
		const testConnection = vi.spyOn(integrationApi, 'testConnectionToRadarrEndpoint').mockReturnValue(success(testResult));

		// Act
		await subscribeSpyTo(store.test()).onComplete();

		// Assert
		expect(testConnection).toHaveBeenCalledWith({ url: 'http://radarr', apiKey: 'key', integrationId: 'id' });
		expect(store.items[0]!.lastConnectionTestStatus).toBe(TestConnectionStatus.Success);
		expect(store.items[0]!.lastConnectionTestedAt).toBe(testResult.testedAt);

		const staleDetail = {
			...store.detail!,
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
		};
		const staleSummary = {
			...store.items[0]!,
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
		};
		vi.spyOn(integrationApi, 'updateRadarrIntegrationEndpoint').mockReturnValue(success(staleDetail));
		const refreshResponse = new Subject<ResultDTO<IntegrationSummary[]>>();
		vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(refreshResponse.asObservable());

		const save = subscribeSpyTo(store.save());
		store.testResult = null;
		refreshResponse.next({ isSuccess: true, value: [staleSummary], errors: [], successes: [], statusCode: 200 });
		refreshResponse.complete();
		await save.onComplete();

		expect(store.items[0]!.lastConnectionTestStatus).toBe(TestConnectionStatus.Success);
	});

	test('Should require a download folder when validating a draft', () => {
		// Arrange
		const store = useIntegrationStore();
		Object.assign(store.draft, {
			name: 'Radarr',
			url: 'http://radarr',
			apiKey: 'key',
			category: 'radarr',
		});

		// Act
		store.draft.downloadFolderId = 0;

		// Assert
		expect(store.isDraftValid).toBe(false);
	});

	test('Should delete a persisted Radarr integration even when external cleanup fails', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.detail = {
			apiKey: 'key',
			category: 'radarr',
			downloadFolderId: 1,
			id: 'id',
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
			name: 'Radarr',
			provisioningState: IntegrationProvisioningState.Configured,
			type: IntegrationType.Radarr,
			url: 'http://radarr',
		};
		const deleteIntegration = vi.spyOn(integrationApi, 'deleteRadarrIntegrationEndpoint').mockReturnValue(success(undefined));
		vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(success([]));

		// Act
		await subscribeSpyTo(store.delete()).onComplete();

		// Assert
		expect(deleteIntegration).toHaveBeenCalledWith('id', { Force: true });
		expect(store.detail).toBeNull();
		expect(store.isDeleting).toBe(false);
	});

	test('Should reject an unsupported integration type instead of using the Sonarr endpoint', () => {
		// Arrange
		const store = useIntegrationStore();
		const saveSonarr = vi.spyOn(integrationApi, 'createSonarrIntegrationEndpoint');
		store.openAdd('Lidarr' as IntegrationType);

		// Act
		const save = () => store.save();

		// Assert
		expect(save).toThrowError('Unsupported integration type: Lidarr');
		expect(saveSonarr).not.toHaveBeenCalled();
	});
});
