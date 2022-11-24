import { cloneDeep } from 'lodash-es';
import { faker } from '@faker-js/faker';
import { MockConfig } from '@mock/interfaces';
import { PlexMediaType } from '@dto/mainApi';

export function checkConfig(config: MockConfig | null = null): MockConfig {
	if (config === null || config === undefined) {
		return checkConfig({});
	}

	const newConfig: MockConfig = cloneDeep(config);
	if (!hasConfigProperty(config, 'plexServerCount')) {
		newConfig.plexServerCount = 5;
	}

	if (!hasConfigProperty(config, 'seed')) {
		newConfig.seed = 1234;
	}

	if (!hasConfigProperty(config, 'plexAccountCount')) {
		newConfig.plexAccountCount = 2;
	}

	if (!hasConfigProperty(config, 'firstTimeSetup')) {
		newConfig.firstTimeSetup = false;
	}

	if (!hasConfigProperty(config, 'plexLibraryCount')) {
		newConfig.plexLibraryCount = 5;
	}

	if (!hasConfigProperty(config, 'plexLibraryTypes')) {
		newConfig.plexLibraryTypes = [PlexMediaType.Movie, PlexMediaType.TvShow];
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

function hasConfigProperty(config: MockConfig, key: keyof MockConfig) {
	return config.hasOwnProperty(key);
}
