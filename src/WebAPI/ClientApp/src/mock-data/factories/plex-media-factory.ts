import { randMovie, randNumber, randRecentDate } from '@ngneat/falso';
import { times } from 'lodash-es';
import { checkConfig, incrementSeed, type MockConfig } from '@mock';
import { PlexMediaType, type PlexLibraryDTO, type PlexMediaSlimDTO } from '@dto/mainApi';

let plexMediaIdIndex = 1;

export function generatePlexMedia({
	id,
	plexLibraryId,
	plexServerId,
	type,
	config = {},
	partialData = {},
}: {
	id: number;
	plexLibraryId: number;
	plexServerId: number;
	type: PlexMediaType;
	partialData?: Partial<PlexLibraryDTO>;
	config?: Partial<MockConfig>;
}): PlexMediaSlimDTO {
	checkConfig(config);
	incrementSeed(id);
	const title = randMovie();
	return {
		id,
		childCount: 0,
		duration: randNumber({ min: 1900, max: 2023 }),
		plexLibraryId,
		sortTitle: title.toLowerCase(),
		title,
		year: randNumber({ min: 1900, max: 2023 }),
		type,
		plexServerId,
		hasThumb: false,
		mediaSize: randNumber({ min: 10000, max: 1000000 }),
		addedAt: randRecentDate({ days: 120 }).toUTCString(),
		updatedAt: randRecentDate({ days: 60 }).toUTCString(),
		index: 0,
		children: [],
		qualities: [],
		fullThumbUrl: 'https://www.omdbapi.com/src/poster.jpg',
		...partialData,
	};
}

export function generatePlexMedias({
	plexLibraryId,
	plexServerId,
	type,
	config = {},
	partialData = {},
}: {
	plexLibraryId: number;
	plexServerId: number;
	type: PlexMediaType;
	partialData?: Partial<PlexMediaSlimDTO>;
	config?: Partial<MockConfig>;
}): PlexMediaSlimDTO[] {
	const validConfig = checkConfig(config);
	let count = 0;
	switch (type) {
		case PlexMediaType.Movie:
			count = validConfig.movieCount;
			break;
		case PlexMediaType.TvShow:
			count = validConfig.tvShowCount;
			break;
		case PlexMediaType.Season:
			count = validConfig.seasonCount;
			break;
		case PlexMediaType.Episode:
			count = validConfig.episodeCount;
			break;
		default:
			throw new Error(`Invalid PlexMediaType: ${type}`);
	}
	let index = 1;
	return times(count, () =>
		generatePlexMedia({
			id: plexMediaIdIndex++,
			plexServerId,
			plexLibraryId,
			type,
			config,
			partialData,
		}),
	)
		.sort((a, b) => a.sortTitle.localeCompare(b.sortTitle))
		.map((x) => {
			return {
				...x,
				index: index++,
			};
		});
}
