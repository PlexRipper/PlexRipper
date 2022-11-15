import { cloneDeep } from 'lodash-es';
import { MockConfig } from './interfaces/MockConfig';

export function checkConfig(config: MockConfig | null = null): MockConfig {
	if (config === null || config === undefined) {
		return checkConfig({});
	}

	const newConfig: MockConfig = cloneDeep(config);
	if (!config.hasOwnProperty('plexServerCount')) {
		newConfig.plexServerCount = 5;
	}

	if (!config.hasOwnProperty('plexAccountCount')) {
		newConfig.plexAccountCount = 2;
	}

	if (!config.hasOwnProperty('firstTimeSetup')) {
		newConfig.firstTimeSetup = false;
	}

	return newConfig;
}
