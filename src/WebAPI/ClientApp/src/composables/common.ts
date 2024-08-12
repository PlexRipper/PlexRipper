import { orderBy } from 'lodash-es';
import type { PlexServerConnectionDTO } from '@dto';

export function sortPlexServerConnections(connections: PlexServerConnectionDTO[]) {
	const keysOrder: (keyof PlexServerConnectionDTO)[] = ['local', 'isPlexTvConnection'];
	return	orderBy(connections, keysOrder, ['asc', 'asc']);
}
