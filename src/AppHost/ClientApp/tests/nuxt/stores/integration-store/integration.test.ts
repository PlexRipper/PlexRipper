import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { of } from 'rxjs';
import { baseSetup, subscribeSpyTo } from '@services-test-base';
import { integrationApi } from '@api';
import { IntegrationProvisioningState, IntegrationType, TestConnectionStatus } from '@dto';
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
		vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(success([staleSummary]));

		await subscribeSpyTo(store.save()).onComplete();

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
