import { faker } from '@faker-js/faker';
import { checkConfig, getId, incrementSeed } from './mock-base';
import { MockConfig } from '@mock/interfaces';
import {
	PlexServerConnectionDTO,
	PlexServerDTO,
	PlexServerStatusDTO,
	ServerConnectionCheckStatusProgressDTO,
} from '@dto/mainApi';

export function generatePlexServers(config: Partial<MockConfig> = {}): PlexServerDTO[] {
	const validConfig = checkConfig(config);

	const plexServers: PlexServerDTO[] = [];

	// @ts-ignore
	for (let i = 0; i < validConfig.plexServerCount; i++) {
		incrementSeed();
		const plexServerId = getId();
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
			ownerId: faker.datatype.number(99999),
			plexServerConnections: generatePlexServerConnection(plexServerId, config),
		});
	}

	return plexServers;
}

export function generatePlexServerStatus(config: Partial<MockConfig> = {}): PlexServerStatusDTO {
	return {
		id: 1,
		plexServerId: 0,
		statusCode: 200,
		statusMessage: 'The Plex server is online!',
		isSuccessful: true,
		plexServerConnectionId: 0,
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
		incrementSeed();
		const connectionId = getId();

		plexServerConnections.push({
			id: connectionId,
			address: host,
			url: `${scheme}://${host}:${port}`,
			iPv6: false,
			local: false,
			relay: false,
			plexServerId,
			port,
			serverStatusList: [],
			protocol: scheme,
			latestConnectionStatus: generatePlexServerStatus(validConfig),
			iPv4: true,
			portFix: false,
			progress: generateServerConnectionCheckStatusProgress(plexServerId, connectionId, config),
		});
	}

	return plexServerConnections;
}

export function generateServerConnectionCheckStatusProgress(
	plexServerId: number,
	plexServerConnectionId: number,
	config: Partial<MockConfig> = {},
): ServerConnectionCheckStatusProgressDTO {
	incrementSeed();

	const completed = faker.datatype.boolean();
	return {
		plexServerId,
		plexServerConnectionId,
		completed,
		connectionSuccessful: faker.datatype.boolean(),
		message: completed ? 'Completed' : 'Running',
		retryAttemptCount: 0,
		retryAttemptIndex: 0,
		statusCode: completed ? 200 : 408,
		timeToNextRetry: 0,
	};
}
