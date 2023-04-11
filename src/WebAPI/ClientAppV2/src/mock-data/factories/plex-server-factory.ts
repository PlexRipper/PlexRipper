import * as Factory from 'factory.ts';
import { randSemver, randNumber, randRecentDate, randPort, randIp, randBrand, randBoolean, randUuid } from '@ngneat/falso';
import { resetSeed } from './utils';
import {
	PlexServerConnectionDTO,
	PlexServerDTO,
	PlexServerStatusDTO,
	ServerConnectionCheckStatusProgressDTO,
} from '@dto/mainApi';
import { checkConfig, MockConfig } from '@mock';

export function generatePlexServers(config: Partial<MockConfig> = {}): PlexServerDTO[] {
	const validConfig = checkConfig(config);
	const plexServerDTOFactory = Factory.Sync.makeFactory<PlexServerDTO>(() => {
		resetSeed();
		return {
			id: Factory.each((i) => i),
			name: randBrand() + ' Server',
			ownerId: randNumber({ max: 999999 }),
			device: 'PC',
			dnsRebindingProtection: randBoolean(),
			home: randBoolean(),
			httpsRequired: randBoolean(),
			lastSeenAt: randRecentDate({ days: 10 }).toUTCString(),
			natLoopbackSupported: randBoolean(),
			owned: randBoolean(),
			platform: 'Linux',
			platformVersion: randSemver({ prefix: 'v' }),
			plexServerOwnerUsername: '',
			preferredConnectionId: 0,
			presence: randBoolean(),
			product: '',
			productVersion: randSemver({ prefix: 'v' }),
			provides: '',
			publicAddress: randIp(),
			publicAddressMatches: randBoolean(),
			relay: randBoolean(),
			synced: randBoolean(),
			machineIdentifier: randUuid(),
			createdAt: randRecentDate({ days: 30 }).toUTCString(),
			plexServerConnections: Factory.each((i) => generatePlexServerConnection(i, validConfig)),
		};
	});

	return plexServerDTOFactory.buildList(validConfig.plexServerCount);
}

export function generatePlexServerConnection(plexServerId: number, config: Partial<MockConfig> = {}): PlexServerConnectionDTO[] {
	const validConfig = checkConfig(config);

	const plexServerConnectionDTOFactory = Factory.Sync.makeFactory<PlexServerConnectionDTO>(() => {
		resetSeed();
		const scheme = 'http';
		const host = randIp();
		const port = randPort();

		return {
			id: Factory.each((i) => i),
			protocol: scheme,
			address: host,
			port,
			url: `${scheme}://${host}:${port}`,
			iPv4: true,
			iPv6: false,
			local: false,
			relay: false,
			portFix: false,
			progress: {} as ServerConnectionCheckStatusProgressDTO,
			serverStatusList: [],
			latestConnectionStatus: {} as PlexServerStatusDTO,
			plexServerId: -1,
		};
	});

	return plexServerConnectionDTOFactory.buildList(validConfig.maxServerConnections, { plexServerId });
}

export function generatePlexServerStatusDTO(plexServerId: number, plexServerConnectionId: number): PlexServerStatusDTO[] {
	const plexServerStatusDTOFactory = Factory.Sync.makeFactory<PlexServerStatusDTO>(() => {
		resetSeed();
		return {
			id: Factory.each((i) => i),
			isSuccessful: true,
			lastChecked: randRecentDate({ days: 10 }).toUTCString(),
			plexServerConnectionId: -1,
			plexServerId: -1,
			statusCode: 200,
			statusMessage: 'Completed',
		};
	});
	return plexServerStatusDTOFactory.buildList(3, { plexServerId, plexServerConnectionId });
}
