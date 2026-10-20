import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { of } from 'rxjs';
import { baseSetup, subscribeSpyTo } from '@services-test-base';
import { integrationApi } from '@api';
import { IntegrationProvisioningState, IntegrationType, TestConnectionStatus, type SonarrIntegrationDTO } from '@dto';
import { generateResultDTO } from '@mock';
import { useIntegrationStore } from '@store';

describe('IntegrationStore saved URL validation', () => {
	beforeAll(baseSetup);

	beforeEach(() => {
		setActivePinia(createPinia());
		vi.restoreAllMocks();
	});

	test('Should preserve an invalid draft without saving and accept its corrected offline URL', async () => {
		// Arrange
		const store = useIntegrationStore();
		store.openAdd(IntegrationType.Sonarr);
		Object.assign(store.draft, { name: 'New Sonarr', url: 'ftp://offline.example:8989/sonarr', apiKey: 'key', category: 'sonarr' });
		const saved: SonarrIntegrationDTO = {
			id: '00000000-0000-0000-0000-000000000061',
			name: 'Stored Sonarr',
			url: 'https://offline.example:8989/sonarr',
			apiKey: 'key',
			category: 'sonarr',
			downloadFolderId: 1,
			provisioningState: IntegrationProvisioningState.Unconfigured,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestErrorMessage: null,
			lastConnectionTestedAt: null,
		};
		const create = vi.spyOn(integrationApi, 'createSonarrIntegrationEndpoint').mockReturnValue(of(generateResultDTO(saved)));
		const update = vi.spyOn(integrationApi, 'updateSonarrIntegrationEndpoint').mockReturnValue(of(generateResultDTO(saved)));
		const createRadarr = vi.spyOn(integrationApi, 'createRadarrIntegrationEndpoint').mockReturnValue(of(generateResultDTO(saved)));
		const updateRadarr = vi.spyOn(integrationApi, 'updateRadarrIntegrationEndpoint').mockReturnValue(of(generateResultDTO(saved)));
		const refresh = vi.spyOn(integrationApi, 'getIntegrationsEndpoint').mockReturnValue(of(generateResultDTO([])));
		const testConnection = vi.spyOn(integrationApi, 'testConnectionToSonarrEndpoint');

		// Act
		const rejected = subscribeSpyTo(store.save());
		await rejected.onComplete();

		// Assert
		expect(store.isDraftValid).toBe(false);
		expect(rejected.getValues()).toEqual([]);
		expect(create).not.toHaveBeenCalled();
		expect(update).not.toHaveBeenCalled();
		expect(createRadarr).not.toHaveBeenCalled();
		expect(updateRadarr).not.toHaveBeenCalled();
		expect(refresh).not.toHaveBeenCalled();
		expect(store.detail).toBeNull();
		expect(store.isSaving).toBe(false);
		expect(store.requiresSetupPrompt).toBe(false);
		expect(store.draft.url).toBe('ftp://offline.example:8989/sonarr');

		// Act
		const correctedUrl = '  https://offline.example:8989/sonarr/  ';
		store.draft.url = correctedUrl;
		expect(store.isDraftValid).toBe(true);
		await subscribeSpyTo(store.save()).onComplete();

		// Assert
		expect(create).toHaveBeenCalledExactlyOnceWith({ name: 'New Sonarr', url: correctedUrl, apiKey: 'key', category: 'sonarr', downloadFolderId: 1 });
		expect(testConnection).not.toHaveBeenCalled();
		expect(store.detail).toEqual({ ...saved, type: IntegrationType.Sonarr });
		expect(store.draft.name).toBe('Stored Sonarr');
		expect(store.requiresSetupPrompt).toBe(true);
		expect(store.isSaving).toBe(false);
	});
});
