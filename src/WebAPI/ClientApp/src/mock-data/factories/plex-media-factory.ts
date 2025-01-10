import { randCompanyName, randMovie, randNumber, randRecentDate, randSentence, randUuid } from '@ngneat/falso';
import { times, uniqueId } from 'lodash-es';
import { checkConfig, incrementSeed, type MockConfig } from '@mock';
import { PlexMediaType, type PlexMediaSlimDTO, type PlexMediaDTO, type PlexMediaStatisticsDTO } from '@dto';

export function generatePlexMediaStatisticsDTO(mediaList: PlexMediaSlimDTO[]): PlexMediaStatisticsDTO {
	return {
		mediaList: mediaList,
		mediaCount: mediaList.length,
		movieCount: mediaList.filter((x) => x.type === PlexMediaType.Movie).length,
		tvShowCount: mediaList.filter((x) => x.type === PlexMediaType.TvShow).length,
		seasonCount: mediaList.filter((x) => x.type === PlexMediaType.Season).length,
		episodeCount: mediaList.filter((x) => x.type === PlexMediaType.Episode).length,
		mediaSize: mediaList.reduce((acc, x) => acc + x.mediaSize, 0),
	};
}

function generatePlexMediaSlim({
	config = {},
	partialData,
}: {
	config?: Partial<MockConfig>;
	partialData: Pick<PlexMediaSlimDTO, 'plexServerId' | 'plexLibraryId' | 'type'> & Partial<PlexMediaSlimDTO>;
}): PlexMediaSlimDTO {
	const id = +uniqueId();
	const validConfig = checkConfig(config);
	incrementSeed(id);
	const title = randMovie();
	const media: PlexMediaSlimDTO = {
		id,
		key: randNumber({ min: 1, max: 1000000 }),
		metaDataKey: randNumber({ min: 1, max: 1000000 }),
		plexToken: randUuid(),
		childCount: 0,
		grandChildCount: 0,
		duration: randNumber({ min: 1900, max: 2023 }),
		plexLibraryId: partialData?.plexLibraryId || 0,
		sortIndex: 0,
		title,
		searchTitle: title.toLowerCase(),
		year: randNumber({ min: 1900, max: 2023 }),
		type: partialData?.type || PlexMediaType.Unknown,
		plexServerId: partialData?.plexServerId || 0,
		hasThumb: false,
		mediaSize: randNumber({ min: 10000, max: 1000000 }),
		addedAt: randRecentDate({ days: 120 }).toUTCString(),
		updatedAt: randRecentDate({ days: 60 }).toUTCString(),
		qualities: [],
	};

	switch (partialData.type) {
		case PlexMediaType.TvShow:
			media.childCount = validConfig.seasonCount;
			media.grandChildCount = validConfig.seasonCount * validConfig.episodeCount;
			break;
		case PlexMediaType.Season:
			media.childCount = validConfig.episodeCount;
			break;
	}

	return Object.assign(media, partialData);
}

export function generatePlexMedia({
	config = {},
	partialData,
}: {
	config?: Partial<MockConfig>;
	partialData: Pick<PlexMediaDTO, 'plexServerId' | 'plexLibraryId' | 'type'> & Partial<PlexMediaDTO>;
}): PlexMediaDTO {
	const validConfig = checkConfig(config);
	const media: PlexMediaDTO = {
		...generatePlexMediaSlim({ config, partialData }),
		hasArt: false,
		hasBanner: false,
		hasTheme: false,
		rating: randNumber({ min: 1, max: 10 }),
		studio: randCompanyName(),
		summary: randSentence(),
		tvShowId: 0,
		tvShowSeasonId: 0,
		mediaData: [],
		children: [],
	};

	switch (partialData.type) {
		case PlexMediaType.TvShow:
			media.tvShowId = media.id;
			media.children = times(validConfig.seasonCount, () =>
				generatePlexMedia({
					config,
					partialData: { ...partialData, id: +uniqueId(), type: PlexMediaType.Season, tvShowId: media.id },
				}),
			);
			break;
		case PlexMediaType.Season:
			media.tvShowSeasonId = media.id;
			media.children = times(validConfig.episodeCount, () =>
				generatePlexMedia({
					config,
					partialData: {
						...partialData,
						id: +uniqueId(),
						type: PlexMediaType.Episode,
						tvShowId: media.tvShowId,
						tvShowSeasonId: media.tvShowSeasonId,
					},
				}),
			);
			break;
	}

	return Object.assign(media, partialData);
}

export function generatePlexMediaSlims({
	config = {},
	partialData,
}: {
	partialData: Partial<PlexMediaSlimDTO> & Pick<PlexMediaSlimDTO, 'plexServerId' | 'plexLibraryId' | 'type'>;
	config?: Partial<MockConfig>;
}): PlexMediaSlimDTO[] {
	const validConfig = checkConfig(config);
	let count = 0;
	switch (partialData.type) {
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
			throw new Error(`Invalid PlexMediaType: ${partialData.type}`);
	}

	let index = 1;
	return times(count, () =>
		generatePlexMediaSlim({
			config,
			partialData,
		}),
	)
		.sort((a, b) => a.sortIndex.toString().localeCompare(b.sortIndex.toString()))
		.map((x) => {
			return {
				...x,
				index: index++,
			};
		});
}

export function generatePlexMedias({
	config = {},
	partialData,
}: {
	partialData: Partial<PlexMediaDTO>;
	config?: Partial<MockConfig>;
}): PlexMediaDTO[] {
	const validConfig = checkConfig(config);
	let count = 0;
	switch (partialData.type) {
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
			throw new Error(`Invalid PlexMediaType: ${partialData.type}`);
	}
	let index = 1;
	return times(count, () =>
		generatePlexMedia({
			config,
			partialData: {
				plexServerId: partialData.plexServerId ?? 0,
				plexLibraryId: partialData.plexLibraryId ?? 0,
				type: partialData.type ?? PlexMediaType.Unknown,
				...partialData,
			},
		}),
	)
		.sort((a, b) => a.sortIndex.toString().localeCompare(b.sortIndex.toString()))
		.map((x) => {
			return {
				...x,
				index: index++,
			};
		});
}
