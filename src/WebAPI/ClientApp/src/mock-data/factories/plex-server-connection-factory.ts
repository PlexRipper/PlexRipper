import { randIp, randPort } from '@ngneat/falso';
import { times } from 'lodash-es';
import { PlexConnectionTypes, type PlexServerConnectionDTO, type PlexServerDTO, type PlexServerStatusDTO } from '@dto';
import { checkConfig, incrementSeed, type MockConfig } from '~/mock-data';

let plexServerConnectionIdIndex = 1;

export function generatePlexServerConnection({
	id,
	plexServerId,
	partialData = {},
}: {
	id: number;
	plexServerId: number;
	partialData?: Partial<PlexServerConnectionDTO>;
}): PlexServerConnectionDTO {
	incrementSeed(id);
	const scheme = 'http';
	const host = randIp();
	const port = randPort();

	return {
		id,
		protocol: scheme,
		address: host,
		port,
		url: `${scheme}://${host}:${port}`,
		isPlexTvConnection: false,
		iPv4: true,
		iPv6: false,
		local: false,
		isCustom: false,
		relay: false,
		latestConnectionStatus: {} as PlexServerStatusDTO,
		plexServerId,
		type: PlexConnectionTypes.Public,
		...partialData,
	};
}

export function generatePlexServerConnections({
	plexServerId,
	config = {},
	partialData = {},
}: {
	plexServerId: number;
	config: Partial<MockConfig>;
	partialData?: Partial<PlexServerDTO>;
}): PlexServerConnectionDTO[] {
	const validConfig = checkConfig(config);
	return times(validConfig.maxServerConnections, () =>
		generatePlexServerConnection({ id: plexServerConnectionIdIndex++, plexServerId, partialData }),
	);
}
