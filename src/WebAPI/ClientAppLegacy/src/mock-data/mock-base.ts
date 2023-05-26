import { cloneDeep } from 'lodash-es';
import { faker } from '@faker-js/faker';
import { MockConfig } from '@mock/interfaces';
import { PlexMediaType } from '@dto/mainApi';

export function checkConfig(config: Partial<MockConfig> = {}): MockConfig {
	if (config === null || config === undefined) {
		return checkConfig({});
	}

	const newConfig: Partial<MockConfig> = cloneDeep(config);
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

	if (!hasConfigProperty(config, 'plexServerAccessCount')) {
		newConfig.plexServerAccessCount = 3;
	}

	if (!hasConfigProperty(config, 'plexLibraryTypes')) {
		newConfig.plexLibraryTypes = [PlexMediaType.Movie, PlexMediaType.TvShow];
	}

	if (!hasConfigProperty(config, 'maxServerConnections')) {
		newConfig.maxServerConnections = 3;
	}

	if (!hasConfigProperty(config, 'connectionHasProgress')) {
		newConfig.connectionHasProgress = false;
	}

	faker.seed(config.seed);

	return newConfig as MockConfig;
}

export function setSeed(seed: number) {
	faker.seed(seed);
}

export function incrementSeed(increment: number = 1) {
	if (increment === 0) {
		return;
	}
	setSeed(faker.seed() + increment);
}

function hasConfigProperty(config: Partial<MockConfig>, key: keyof MockConfig) {
	return config.hasOwnProperty(key);
}

export function getId(): number {
	return faker.datatype.number({ min: 1, max: 99999999999 });
}
