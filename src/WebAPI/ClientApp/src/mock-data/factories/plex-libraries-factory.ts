import { randCompanyName, randNumber, randRecentDate, randUuid } from '@ngneat/falso';
import { times } from 'lodash-es';
import { checkConfig, incrementSeed, type MockConfig } from '@mock';
import { PlexMediaType, type FolderPathDTO, type PlexLibraryDTO, type PlexServerDTO } from '@dto';

let plexLibraryIdIndex = 1;

export function generatePlexLibrary({
	id,
	plexServerId,
	type,
	config = {},
	partialData = {},
}: {
	id: number;
	plexServerId: number;
	type: PlexMediaType;
	partialData?: Partial<PlexLibraryDTO>;
	config?: Partial<MockConfig>;
}): PlexLibraryDTO {
	checkConfig(config);
	incrementSeed(id);
	return {
		id,
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
	plexServerId,
	type,
	config = {},
	partialData = {},
}: {
	plexServerId: number;
	type: PlexMediaType;
	partialData?: Partial<PlexLibraryDTO>;
	config?: Partial<MockConfig>;
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
	return times(count, () => generatePlexLibrary({ id: plexLibraryIdIndex++, type, plexServerId, partialData }));
}

export function generatePlexLibrariesFromPlexServers({
	plexServers,
	config = {},
}: {
	plexServers: PlexServerDTO[];
	config?: Partial<MockConfig>;
}): PlexLibraryDTO[] {
	return plexServers
		.map((x) => {
			return [
				...generatePlexLibraries({
					type: PlexMediaType.Movie,
					config,
					plexServerId: x.id,
				}),
				...generatePlexLibraries({
					type: PlexMediaType.TvShow,
					config,
					plexServerId: x.id,
				}),
			];
		})
		.flat();
}
