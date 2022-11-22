import { cloneDeep } from 'lodash-es';
import { faker } from '@faker-js/faker';
import { MockConfig } from '@mock/interfaces';

export function checkConfig(config: MockConfig | null = null): MockConfig {
	if (config === null || config === undefined) {
		return checkConfig({});
	}

	const newConfig: MockConfig = cloneDeep(config);
	if (!config.hasOwnProperty('plexServerCount')) {
		newConfig.plexServerCount = 5;
	}

	if (!config.hasOwnProperty('seed')) {
		newConfig.seed = 1234;
	}

	if (!config.hasOwnProperty('plexAccountCount')) {
		newConfig.plexAccountCount = 2;
	}

	if (!config.hasOwnProperty('firstTimeSetup')) {
		newConfig.firstTimeSetup = false;
	}

	faker.seed(config.seed);

	return newConfig;
}

export function setSeed(seed: number) {
	faker.seed(seed);
}

export function incrementSeed(increment: number) {
	if (increment === 0) {
		return;
	}
	setSeed(faker.seed() + increment);
}
