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

	test('opens a new typed draft', () => {
		const store = useIntegrationStore();

		store.openAdd(IntegrationType.Radarr);

		expect(store.draft).toMatchObject({ type: IntegrationType.Radarr, name: '', url: '', apiKey: '' });
	});

	test('creates a Sonarr integration through the typed endpoint', () => {
		const store = useIntegrationStore();
		store.openAdd(IntegrationType.Sonarr);
		Object.assign(store.draft, { name: 'Sonarr', url: 'http://sonarr', apiKey: 'key', category: 'sonarr' });
		const detail = {
			id: 'id', name: 'Sonarr', url: 'http://sonarr', apiKey: 'key', category: 'sonarr', downloadFolderId: null,
			provisioningState: IntegrationProvisioningState.Unconfigured,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
		};
		const create = vi.spyOn(integrationApi, 'createSonarrIntegrationEndpoint').mockReturnValue(success(detail));
		vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(success([]));

		const result = subscribeSpyTo(store.save());

		expect(result.getLastValue()?.isSuccess).toBe(true);
		expect(create).toHaveBeenCalledWith({
			name: 'Sonarr',
			url: 'http://sonarr',
			apiKey: 'key',
			category: 'sonarr',
			downloadFolderId: null,
		});
		expect(store.detail?.type).toBe(IntegrationType.Sonarr);
		expect(store.requiresSetupPrompt).toBe(true);
	});

	test('tests a Radarr draft through the typed endpoint', () => {
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

		subscribeSpyTo(store.test());

		expect(testConnection).toHaveBeenCalledWith({ url: 'http://radarr', apiKey: 'key' });
		expect(store.testResult).toEqual(testResult);
	});
});
