import { randCompanyName, randNumber, randRecentDate, randUuid } from '@ngneat/falso';
import { times } from 'lodash-es';
import type { Seed, MockConfig } from '@mock';
import { checkConfig, randId } from '@mock';
import { PlexMediaType, type FolderPathDTO, type PlexLibraryDTO, type PlexServerDTO } from '@dto';

export function generatePlexLibrary({
	seed,
	plexServerId,
	type,
	config = {},
	partialData = {},
}: {
	plexServerId: number;
	type: PlexMediaType;
	partialData?: Partial<PlexLibraryDTO>;
	config?: Partial<MockConfig>;
	seed: Seed;
}): PlexLibraryDTO {
	checkConfig(config);
	seed.next();
	return {
		id: randId(),
		type,
		title: randCompanyName(),
		key: '' + randNumber({ max: 999999 }),
		defaultDestination: {} as FolderPathDTO,
		createdAt: randRecentDate({ days: 60 }).toUTCString(),
		updatedAt: randRecentDate({ days: 10 }).toUTCString(),
		scannedAt: randRecentDate({ days: 10 }).toUTCString(),
		syncedAt: randRecentDate({ days: 10 }).toUTCString(),
		uuid: randUuid(),
		count: 0,
		defaultDestinationId: 0,
		episodeCount: 0,
		mediaSize: 0,
		outdated: false,
		plexServerId,
		seasonCount: 0,
		...partialData,
	};
}

export function generatePlexLibraries({
	seed,
	plexServerId,
	type,
	config = {},
	partialData = {},
}: {
	plexServerId: number;
	type: PlexMediaType;
	partialData?: Partial<PlexLibraryDTO>;
	config?: Partial<MockConfig>;
	seed: Seed;
}): PlexLibraryDTO[] {
	const validConfig = checkConfig(config);

	let count = 0;
	switch (type) {
		case PlexMediaType.Movie:
			count = validConfig.plexMovieLibraryCount;
			break;

		case PlexMediaType.TvShow:
			count = validConfig.plexTvShowLibraryCount;
			break;
		default:
			throw new Error(`Invalid Plex media type: ${type}`);
	}
	return times(count, () => generatePlexLibrary({ seed, type, plexServerId, partialData }));
}

export function generatePlexLibrariesFromPlexServers({
	seed,
	plexServers,
	config = {},
}: {
	plexServers: PlexServerDTO[];
	config?: Partial<MockConfig>;
	seed: Seed;
}): PlexLibraryDTO[] {
	return plexServers
		.map((x) => {
			return [
				...generatePlexLibraries({
					seed,
					type: PlexMediaType.Movie,
					config,
					plexServerId: x.id,
				}),
				...generatePlexLibraries({
					seed,
					type: PlexMediaType.TvShow,
					config,
					plexServerId: x.id,
				}),
			];
		})
		.flat();
}
