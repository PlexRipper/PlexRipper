import type { BasePageSetupResult } from '@fixtures';
import type { MockConfig } from '@mock';
import { generateResultDTO, generateSettingsModel } from '@mock';
import { SettingsPaths } from '@api/api-paths';
import { headers } from '@fixtures';

export function setupMockSettingsEndpoints(
	this: BasePageSetupResult,
	config: MockConfig,
): BasePageSetupResult {
	this.settings = generateSettingsModel({
		plexServers: this.plexServers, config,
	});
	this.settings.generalSettings.hasBeenInvitedToDiscord = true;

	if (config.override.settings) {
		this.settings = config.override.settings(this.settings);
	}
	cy.intercept('GET', SettingsPaths.getUserSettingsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(this.settings),
		...headers,
	}).then(() => {
		if (config.debugDisplayData) {
			cy.log('BasePageSetup -> settings', this.settings);
		}
	});

	return this;
}
