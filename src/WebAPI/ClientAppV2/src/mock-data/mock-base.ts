import { seed as falsoSeed } from '@ngneat/falso';
import { MockConfig } from '@mock/interfaces';
import { PlexMediaType } from '@dto/mainApi';

export function checkConfig(config: Partial<MockConfig> = {}): MockConfig {
	if (config === null || config === undefined) {
		return checkConfig({});
	}

	const defaultConfig: MockConfig = {
		plexServerCount: 5,
		seed: 1234,
		debugDisplayData: false,
		plexAccountCount: 1,
		firstTimeSetup: false,
		plexLibraryCount: 5,
		plexServerAccessCount: 3,
		plexServerStatusCount: 3,
		plexLibraryTypes: [PlexMediaType.Movie, PlexMediaType.TvShow],
		movieDownloadTask: 5,
		tvShowDownloadTask: 5,
		seasonDownloadTask: 5,
		episodeDownloadTask: 5,
		connectionHasProgress: false,
		maxServerConnections: 5,
	};

	for (const configKey in config) {
		if (!Object.hasOwn(config, configKey)) {
			config[configKey] = defaultConfig[configKey];
		}
	}

	return config as MockConfig;
}

export function setSeed(seed: number) {
	falsoSeed('' + seed);
}
