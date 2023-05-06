import * as Factory from 'factory.ts';
import { randBoolean, randBrand, randIp, randNumber, randRecentDate, randSemver, randUuid } from '@ngneat/falso';
import { resetSeed } from './utils';
import { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import { checkConfig, MockConfig } from '@mock';
import { generatePlexServerConnections } from '@factories/plex-server-connection-factory';

let plexServerIdIndex = 1;

export function generatePlexServer(
	id: number,
	config: Partial<MockConfig> = {},
	partialData?: Partial<PlexServerDTO>,
): PlexServerDTO {
	const validConfig = checkConfig(config);

	return {
		id,
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
		plexServerConnections: generatePlexServerConnections({ plexServerId: id, config }),
		...partialData,
	};
}

export function generatePlexServers(config: Partial<MockConfig> = {}): PlexServerDTO[] {
	const validConfig = checkConfig(config);
	return Array(validConfig.plexServerCount)
		.fill(null)
		.map(() => generatePlexServer(plexServerIdIndex++, config));
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
