import { faker } from '@faker-js/faker';
import { MockConfig } from './interfaces/MockConfig';
import { checkConfig } from './mock-base';
import { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';

export function generatePlexServers(config: MockConfig | null = null): PlexServerDTO[] {
	config = checkConfig(config);

	const plexServers: PlexServerDTO[] = [];
	// @ts-ignore
	for (let i = 0; i < config.plexServerCount; i++) {
		const scheme = 'http';
		const host = faker.internet.ipv4();
		const port = faker.internet.port();
		const url = `${scheme}://${host}:${port}`;

		plexServers.push({
			id: i + 1,
			name: faker.company.name(),
			scheme,
			address: faker.internet.ipv4(),
			port,
			version: faker.system.semver(),
			host,
			localAddresses: faker.internet.ip(),
			machineIdentifier: faker.datatype.uuid(),
			createdAt: faker.date.past().toUTCString(),
			updatedAt: faker.date.recent().toUTCString(),
			plexLibraries: [],
			downloadTasks: [],
			status: generatePlexServerStatus(config),
			ownerId: faker.datatype.number(99999),
			serverUrl: url,
		});
	}

	return plexServers;
}

export function generatePlexServerStatus(config: MockConfig | null = null): PlexServerStatusDTO {
	config = checkConfig(config);

	return {
		id: 1,
		plexServerId: 0,
		statusCode: 200,
		statusMessage: 'The Plex server is online!',
		isSuccessful: true,
		lastChecked: faker.date.recent().toUTCString(),
	};
}
