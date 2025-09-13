import { randBoolean, randBrand, randIp, randNumber, randRecentDate, randSemver, randUuid } from '@ngneat/falso';
import { times } from 'lodash-es';
import type { PlexServerDTO } from '@dto';
import { checkConfig, incrementSeed, type MockConfig } from '@mock';

let plexServerIdIndex = 1;

export function generatePlexServer({
	id,
	config = {},
	partialData = {},
}: {
	id: number;
	config?: Partial<MockConfig>;
	partialData?: Partial<PlexServerDTO>;
}): PlexServerDTO {
	checkConfig(config);
	incrementSeed(id);

	return {
		id,
		isEnabled: true,
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
		...partialData,
	};
}

export function generatePlexServers({
	config = {},
	partialData = {},
}: {
	config: Partial<MockConfig>;
	partialData?: Partial<PlexServerDTO>;
}): PlexServerDTO[] {
	const validConfig = checkConfig(config);
	return times(validConfig.plexServerCount, () => generatePlexServer({ id: plexServerIdIndex++, config, partialData }));
}
