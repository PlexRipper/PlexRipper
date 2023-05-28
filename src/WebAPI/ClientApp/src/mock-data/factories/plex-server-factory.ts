import { randBoolean, randBrand, randIp, randNumber, randRecentDate, randSemver, randUuid } from '@ngneat/falso';
import { PlexServerConnectionDTO, PlexServerDTO } from '@dto/mainApi';
import { checkConfig, incrementSeed, MockConfig } from '@mock';
import { generatePlexServerConnections } from '@factories/plex-server-connection-factory';

let plexServerIdIndex = 1;

export function generatePlexServer({
	id,
	config = {},
	partialData = {},
}: {
	id: number;
	config: Partial<MockConfig>;
	partialData?: Partial<PlexServerConnectionDTO>;
}): PlexServerDTO {
	checkConfig(config);
	incrementSeed(id);

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

export function generatePlexServers({
	config = {},
	partialData = {},
}: {
	config: Partial<MockConfig>;
	partialData?: Partial<PlexServerConnectionDTO>;
}): PlexServerDTO[] {
	const validConfig = checkConfig(config);
	return Array(validConfig.plexServerCount)
		.fill(null)
		.map(() => generatePlexServer({ id: plexServerIdIndex++, config, partialData }));
}
