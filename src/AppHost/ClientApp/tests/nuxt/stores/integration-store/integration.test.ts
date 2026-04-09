import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { throwError } from 'rxjs';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateFailedResultDTO } from '@mock';
import { integrationApi } from '@api';
import { useIntegrationStore, useSettingsStore } from '@store';

describe('IntegrationStore regressions', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should clear Sonarr testing state when the test request throws', async () => {
		// Arrange
		const integrationStore = useIntegrationStore();
		vi.spyOn(integrationApi, 'testConnectionToSonarrEndpoint').mockReturnValue(throwError(() => new Error('Sonarr failed')) as never);

		// Act
		const result = subscribeSpyTo(integrationStore.testConnectionToSonarr(), { expectErrors: true });
		await result.onError();

		// Assert
		expect(integrationStore.sonarr.isTesting).toBe(false);
	});

	test('Should clear Radarr testing state when the test request throws', async () => {
		// Arrange
		const integrationStore = useIntegrationStore();
		vi.spyOn(integrationApi, 'testConnectionToRadarrEndpoint').mockReturnValue(throwError(() => new Error('Radarr failed')) as never);

		// Act
		const result = subscribeSpyTo(integrationStore.testConnectionToRadarr(), { expectErrors: true });
		await result.onError();

		// Assert
		expect(integrationStore.radarr.isTesting).toBe(false);
	});

	test('Should clear Sonarr configuring state when the configure request throws', async () => {
		// Arrange
		const integrationStore = useIntegrationStore();
		vi.spyOn(integrationApi, 'configureSonarrIntegrationEndpoint').mockReturnValue(throwError(() => new Error('Sonarr configure failed')) as never);

		// Act
		const result = subscribeSpyTo(integrationStore.configureSonarrIntegration(), { expectErrors: true });
		await result.onError();

		// Assert
		expect(integrationStore.sonarr.isConfiguring).toBe(false);
	});

	test('Should clear Radarr configuring state when the configure request throws', async () => {
		// Arrange
		const integrationStore = useIntegrationStore();
		vi.spyOn(integrationApi, 'configureRadarrIntegrationEndpoint').mockReturnValue(throwError(() => new Error('Radarr configure failed')) as never);

		// Act
		const result = subscribeSpyTo(integrationStore.configureRadarrIntegration(), { expectErrors: true });
		await result.onError();

		// Assert
		expect(integrationStore.radarr.isConfiguring).toBe(false);
	});

	test('Should not refresh settings when clearing Radarr configuration fails', async () => {
		// Arrange
		const integrationStore = useIntegrationStore();
		const settingsStore = useSettingsStore();
		settingsStore.integrationsSettings.radarr.radarrBaseUrl = 'http://radarr';
		settingsStore.integrationsSettings.radarr.radarrApiKey = 'A'.repeat(32);
		mock.onDelete('/api/Integration/Radarr/Configuration').reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(integrationStore.clearRadarrConfiguration());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toMatchObject({ isSuccess: false });
		expect(mock.history.get.filter((request) => request.url === '/api/Settings')).toHaveLength(0);
	});

	test('Should not refresh settings when clearing Sonarr configuration fails', async () => {
		// Arrange
		const integrationStore = useIntegrationStore();
		const settingsStore = useSettingsStore();
		settingsStore.integrationsSettings.sonarr.sonarrBaseUrl = 'http://sonarr';
		settingsStore.integrationsSettings.sonarr.sonarrApiKey = 'B'.repeat(32);
		mock.onDelete('/api/Integration/Sonarr/Configuration').reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(integrationStore.clearSonarrConfiguration());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toMatchObject({ isSuccess: false });
		expect(mock.history.get.filter((request) => request.url === '/api/Settings')).toHaveLength(0);
	});
});
