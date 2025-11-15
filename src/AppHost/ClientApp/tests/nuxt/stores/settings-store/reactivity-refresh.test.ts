import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { generateResultDTO, generateSettingsModel } from '@mock';
import { SettingsPaths } from '@api-urls';
import { useSettingsStore } from '@store';

describe('SettingsStore.refreshSettings() deep reactivity', () => {
	// eslint-disable-next-line prefer-const
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should preserve nested reactivity and update integrationsSettings after refresh', async () => {
		// Arrange — seed initial state locally (no server call yet)
		const settingsStore = useSettingsStore();
		const initialSettings = generateSettingsModel({ config });
		initialSettings.integrationsSettings.sonarr.isConfigured = false;
		initialSettings.integrationsSettings.sonarr.sonarrApiKey = 'old-sonarr-key';
		initialSettings.integrationsSettings.sonarr.sonarrBaseUrl = 'http://old-sonarr';
		initialSettings.integrationsSettings.radarr.isConfigured = false;
		initialSettings.integrationsSettings.radarr.radarrApiKey = 'old-radarr-key';
		initialSettings.integrationsSettings.radarr.radarrBaseUrl = 'http://old-radarr';
		settingsStore.setSettingsState(initialSettings);

		// Verify initial state
		expect(settingsStore.integrationsSettings.sonarr.isConfigured).toBe(false);
		expect(settingsStore.integrationsSettings.radarr.isConfigured).toBe(false);

		// Capture nested references before refresh — these should remain reactive
		const sonarrRef = settingsStore.integrationsSettings.sonarr;
		const radarrRef = settingsStore.integrationsSettings.radarr;

		// Prepare server response with updated values
		const updatedSettings = generateSettingsModel({ config });
		updatedSettings.integrationsSettings.sonarr.isConfigured = true;
		updatedSettings.integrationsSettings.sonarr.sonarrApiKey = 'new-sonarr-key';
		updatedSettings.integrationsSettings.sonarr.sonarrBaseUrl = 'http://new-sonarr';
		updatedSettings.integrationsSettings.radarr.isConfigured = true;
		updatedSettings.integrationsSettings.radarr.radarrApiKey = 'new-radarr-key';
		updatedSettings.integrationsSettings.radarr.radarrBaseUrl = 'http://new-radarr';

		mock
			.onGet(SettingsPaths.getUserSettingsEndpoint())
			.reply(200, generateResultDTO(updatedSettings));

		// Act — refresh from server (should mutate nested objects in place)
		const result = subscribeSpyTo(settingsStore.refreshSettings());
		await result.onComplete();

		// Assert — captured references see updated values (identity preserved, deep reactivity intact)
		expect(sonarrRef).toBe(settingsStore.integrationsSettings.sonarr);
		expect(radarrRef).toBe(settingsStore.integrationsSettings.radarr);

		// Critical: isConfigured must be updated (this is what the component checks)
		expect(sonarrRef.isConfigured).toBe(true);
		expect(radarrRef.isConfigured).toBe(true);
		expect(settingsStore.integrationsSettings.sonarr.isConfigured).toBe(true);
		expect(settingsStore.integrationsSettings.radarr.isConfigured).toBe(true);

		expect(settingsStore.integrationsSettings.sonarr.sonarrApiKey).toBe('new-sonarr-key');
		expect(settingsStore.integrationsSettings.sonarr.sonarrBaseUrl).toBe('http://new-sonarr');
		expect(settingsStore.integrationsSettings.radarr.radarrApiKey).toBe('new-radarr-key');
		expect(settingsStore.integrationsSettings.radarr.radarrBaseUrl).toBe('http://new-radarr');
	});

	test('Should update isConfigured for both Sonarr and Radarr when refreshSettings is called as side effect (simulating component scenario)', async () => {
		// Arrange — simulate the component scenario where isConfigured starts as false for both integrations
		const settingsStore = useSettingsStore();
		const initialSettings = generateSettingsModel({ config });
		initialSettings.integrationsSettings.sonarr.isConfigured = false;
		initialSettings.integrationsSettings.sonarr.sonarrApiKey = 'test-sonarr-key';
		initialSettings.integrationsSettings.sonarr.sonarrBaseUrl = 'http://test-sonarr';
		initialSettings.integrationsSettings.radarr.isConfigured = false;
		initialSettings.integrationsSettings.radarr.radarrApiKey = 'test-radarr-key';
		initialSettings.integrationsSettings.radarr.radarrBaseUrl = 'http://test-radarr';
		settingsStore.setSettingsState(initialSettings);

		// Verify initial state matches component expectation for both integrations
		expect(settingsStore.integrationsSettings.sonarr.isConfigured).toBe(false);
		expect(settingsStore.integrationsSettings.sonarr.sonarrApiKey).toBe('test-sonarr-key');
		expect(settingsStore.integrationsSettings.sonarr.sonarrBaseUrl).toBe('http://test-sonarr');
		expect(settingsStore.integrationsSettings.radarr.isConfigured).toBe(false);
		expect(settingsStore.integrationsSettings.radarr.radarrApiKey).toBe('test-radarr-key');
		expect(settingsStore.integrationsSettings.radarr.radarrBaseUrl).toBe('http://test-radarr');

		// Simulate what the component does: capture references to check isConfigured for both integrations
		const sonarrSettingsRef = settingsStore.integrationsSettings.sonarr;
		const radarrSettingsRef = settingsStore.integrationsSettings.radarr;

		// Prepare server response with isConfigured = true (after successful configuration) for both integrations
		const updatedSettings = generateSettingsModel({ config });
		updatedSettings.integrationsSettings.sonarr.isConfigured = true;
		updatedSettings.integrationsSettings.sonarr.sonarrApiKey = 'test-sonarr-key';
		updatedSettings.integrationsSettings.sonarr.sonarrBaseUrl = 'http://test-sonarr';
		updatedSettings.integrationsSettings.radarr.isConfigured = true;
		updatedSettings.integrationsSettings.radarr.radarrApiKey = 'test-radarr-key';
		updatedSettings.integrationsSettings.radarr.radarrBaseUrl = 'http://test-radarr';

		mock
			.onGet(SettingsPaths.getUserSettingsEndpoint())
			.reply(200, generateResultDTO(updatedSettings));

		// Act — simulate component calling refreshSettings() as side effect
		const result = subscribeSpyTo(settingsStore.refreshSettings());
		await result.onComplete();

		// Assert — isConfigured must be true after refresh for both Sonarr and Radarr (this is what components check)
		// Verify Sonarr integration
		expect(sonarrSettingsRef.isConfigured).toBe(true);
		expect(settingsStore.integrationsSettings.sonarr.isConfigured).toBe(true);
		expect(settingsStore.integrationsSettings.sonarr.sonarrApiKey).toBe('test-sonarr-key');
		expect(settingsStore.integrationsSettings.sonarr.sonarrBaseUrl).toBe('http://test-sonarr');
		// Verify Radarr integration
		expect(radarrSettingsRef.isConfigured).toBe(true);
		expect(settingsStore.integrationsSettings.radarr.isConfigured).toBe(true);
		expect(settingsStore.integrationsSettings.radarr.radarrApiKey).toBe('test-radarr-key');
		expect(settingsStore.integrationsSettings.radarr.radarrBaseUrl).toBe('http://test-radarr');
		// Verify the references are still the same objects for both integrations
		expect(sonarrSettingsRef).toBe(settingsStore.integrationsSettings.sonarr);
		expect(radarrSettingsRef).toBe(settingsStore.integrationsSettings.radarr);
	});
});
