import { faker } from '@faker-js/faker';
import { MockConfig } from '@mock/interfaces';
import { generatePlexServers } from '@mock/mock-plex-server';
import { generatePlexLibraries } from '@mock/mock-plex-library';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { incrementSeed } from '@mock/mock-base';

export function generatePlexServersAndLibraries(config: MockConfig | null = null): {
	servers: PlexServerDTO[];
	libraries: PlexLibraryDTO[];
} {
	const servers = generatePlexServers(config);
	const libraries = generatePlexLibraries(config);

	for (let i = 0; i < libraries.length; i++) {
		incrementSeed(i);
		libraries[i].plexServerId = faker.helpers.arrayElement<PlexServerDTO>(servers).id;
	}

	return {
		servers,
		libraries,
	};
}
