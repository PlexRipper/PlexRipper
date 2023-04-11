import { faker } from '@faker-js/faker';
import { generatePlexServers, generatePlexLibraries } from '@factories';
import { MockConfig } from '@mock/interfaces';
import { PlexAccountDTO, PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { checkConfig, incrementSeed } from '@mock/mock-base';
import { generatePlexAccounts } from '@mock/mock-plex-account';

export function generatePlexServersAndLibraries(config: Partial<MockConfig> = {}): {
	servers: PlexServerDTO[];
	libraries: PlexLibraryDTO[];
} {
	const servers = generatePlexServers(config);
	const libraries = generatePlexLibraries(-1, config);

	for (let i = 0; i < libraries.length; i++) {
		incrementSeed(i);
		libraries[i].plexServerId = faker.helpers.arrayElement<PlexServerDTO>(servers).id;
	}

	return {
		servers,
		libraries,
	};
}

export function generatePlexAccountServerAndLibraries(config: Partial<MockConfig> = {}): {
	account: PlexAccountDTO;
	servers: PlexServerDTO[];
	libraries: PlexLibraryDTO[];
} {
	const validConfig = checkConfig(config);

	const { servers, libraries } = generatePlexServersAndLibraries(validConfig);
	const account = generatePlexAccounts(validConfig)[0];

	for (let i = 0; i < Math.min(validConfig.plexServerAccessCount, servers.length - 1); i++) {
		const serverId = faker.helpers.arrayElement<PlexServerDTO>(servers).id;
		account.plexServerAccess.push({
			plexServerId: serverId,
			plexLibraryIds: libraries.filter((x) => x.plexServerId === serverId).map((x) => x.id),
		});
	}

	return {
		servers,
		libraries,
		account,
	};
}
