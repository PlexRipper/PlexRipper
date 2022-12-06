import { faker } from '@faker-js/faker';
import { checkConfig, incrementSeed } from './mock-base';
import { MockConfig } from '@mock/interfaces';
import { PlexServerConnectionDTO, PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';

export function generatePlexServers(config: Partial<MockConfig> = {}): PlexServerDTO[] {
	const validConfig = checkConfig(config);

	const plexServers: PlexServerDTO[] = [];

	// @ts-ignore
	for (let i = 0; i < validConfig.plexServerCount; i++) {
		incrementSeed(i);
		const plexServerId = i + 1;
		plexServers.push({
			id: plexServerId,
			device: '',
			dnsRebindingProtection: faker.datatype.boolean(),
			home: faker.datatype.boolean(),
			httpsRequired: faker.datatype.boolean(),
			lastSeenAt: faker.date.recent().toUTCString(),
			natLoopbackSupported: faker.datatype.boolean(),
			owned: faker.datatype.boolean(),
			platform: '',
			platformVersion: faker.system.semver(),
			plexServerOwnerUsername: '',
			preferredConnectionId: 0,
			presence: faker.datatype.boolean(),
			product: '',
			productVersion: '',
			provides: '',
			publicAddress: faker.internet.ip(),
			publicAddressMatches: faker.datatype.boolean(),
			relay: faker.datatype.boolean(),
			synced: faker.datatype.boolean(),
			name: faker.company.name(),
			machineIdentifier: faker.datatype.uuid(),
			createdAt: faker.date.past().toUTCString(),
			downloadTasks: [],
			status: generatePlexServerStatus(validConfig),
			ownerId: faker.datatype.number(99999),
			plexServerConnections: generatePlexServerConnection(plexServerId, config),
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

export function generatePlexServerConnection(plexServerId: number, config: Partial<MockConfig> = {}): PlexServerConnectionDTO[] {
	const validConfig = checkConfig(config);

	const plexServerConnections: PlexServerConnectionDTO[] = [];
	const scheme = 'http';
	const host = faker.internet.ipv4();
	const port = faker.internet.port();

	for (let i = 0; i < validConfig.maxServerConnections; i++) {
		plexServerConnections.push({
			id: 1 + i,
			address: host,
			url: `${scheme}://${host}:${port}`,
			iPv6: false,
			local: false,
			relay: false,
			plexServerId,
			port,
			protocol: scheme,
			plexServerStatus: [generatePlexServerStatus(validConfig)],
		});
	}

	return plexServerConnections;
}
