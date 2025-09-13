import { randRecentDate } from '@ngneat/falso';
import { times } from 'lodash-es';
import type { PlexServerStatusDTO } from '@dto';
import { checkConfig, type MockConfig } from '@mock';

let plexServerStatusIndexId = 1;

export function generatePlexServerStatus({
	id,
	plexServerId,
	plexServerConnectionId,
	config = {},
}: {
	id: number;
	plexServerId: number;
	plexServerConnectionId: number;
	config: Partial<MockConfig>;
}): PlexServerStatusDTO {
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	const validConfig = checkConfig(config);

	return {
		id,
		isSuccessful: true,
		lastChecked: randRecentDate({ days: 10 }).toUTCString(),
		plexServerConnectionId,
		plexServerId,
		statusCode: 200,
		statusMessage: 'Completed',
	};
}

export function generatePlexServerStatuses({
	plexServerId,
	plexServerConnectionId,
	config = {},
}: {
	plexServerId: number;
	plexServerConnectionId: number;
	config: Partial<MockConfig>;
}): PlexServerStatusDTO[] {
	const validConfig = checkConfig(config);
	return times(validConfig.plexServerStatusCount, () =>
		generatePlexServerStatus({ id: plexServerStatusIndexId++, plexServerId, plexServerConnectionId, config }),
	);
}
