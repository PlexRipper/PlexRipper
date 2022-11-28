import { faker } from '@faker-js/faker';
import { checkConfig, incrementSeed } from './mock-base';
import { MockConfig } from '@mock/interfaces';
import { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';

export function generatePlexServers(config: Partial<MockConfig> = {}): PlexServerDTO[] {
	const validConfig = checkConfig(config);

	const plexServers: PlexServerDTO[] = [];

	// @ts-ignore
	for (let i = 0; i < validConfig.plexServerCount; i++) {
		incrementSeed(i);
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
			downloadTasks: [],
			status: generatePlexServerStatus(validConfig),
			ownerId: faker.datatype.number(99999),
			serverUrl: url,
		});
	}

	return plexServers;
}

export function generatePlexServerStatus(config: Partial<MockConfig> = {}): PlexServerStatusDTO {
	const validConfig = checkConfig(config);

	return {
		id: 1,
		plexServerId: 0,
		statusCode: 200,
		statusMessage: 'The Plex server is online!',
		isSuccessful: true,
		lastChecked: faker.date.recent().toUTCString(),
	};
}
