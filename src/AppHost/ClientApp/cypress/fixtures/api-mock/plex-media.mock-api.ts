import { type BasePageSetupResult, headers } from '@fixtures';
import type { MockConfig } from '@mock';
import { generatePlexMedia, generatePlexMediaSlims, generateResultDTO } from '@mock';
import { PlexMediaPaths } from '@api/api-paths';
import { type PlexMediaStatisticsDTO, PlexMediaType } from '@dto';

export function setupMockPlexMediaEndpoints(
	this: BasePageSetupResult,
	config: MockConfig,
): BasePageSetupResult {
	// Generate library media page data
	for (const library of this.plexLibraries) {
		const mediaList = generatePlexMediaSlims({
			config,
			partialData: {
				plexLibraryId: library.id,
				plexServerId: library.plexServerId,
				type: library.type,
			},
		});

		this.mediaData.push({
			libraryId: library.id,
			media: mediaList,
		});

		for (const mediaItem of mediaList) {
			cy.intercept(
				'GET',
				PlexMediaPaths.getMediaComparisonDetailsEndpoint(mediaItem.id, {
					type: library.type,
				}),
				{
					statusCode: 200,
					body: generateResultDTO({
						plexMediaId: mediaItem.id,
						type: mediaItem.type,
						state: mediaItem.comparisonState,
						rows: [],
					}),
					...headers,
				},
			);

			if (mediaItem.type === PlexMediaType.TvShow) {
				cy.intercept(
					'GET',
					PlexMediaPaths.getMediaDetailByIdEndpoint(mediaItem.id, {
						type: library.type,
					}),
					{
						statusCode: 200,
						body: generateResultDTO(
							generatePlexMedia({
								config,
								partialData: {
									type: PlexMediaType.TvShow,
									id: mediaItem.id,
									plexLibraryId: library.id,
									plexServerId: library.plexServerId,
								},
							}),
						),
						...headers,
					},
				);
			}
		}
	}

	cy.intercept('GET', '**/api/PlexMedia*', (req) => {
		const requestedLibraryId = Number(req.query.plexLibraryId ?? 0);
		const requestedMediaType = String(req.query.mediaType ?? 'None') as PlexMediaType;
		const requestedPage = Number(req.query.page ?? 1);
		const requestedSize = Number(req.query.size ?? 100);

		const page = Number.isFinite(requestedPage) && requestedPage > 0 ? requestedPage : 1;
		const size = Number.isFinite(requestedSize) && requestedSize > 0 ? requestedSize : 100;

		const library = this.plexLibraries.find((x) => x.id === requestedLibraryId);
		const mediaData = this.mediaData.find((x) => x.libraryId === requestedLibraryId);
		const sourceMedia = requestedLibraryId === 0
			? this.mediaData.flatMap((x) => x.media)
			: (mediaData?.media ?? []);
		const allLibraryMedia = [...sourceMedia]
			.filter((item) => requestedMediaType === PlexMediaType.None || item.type === requestedMediaType)
			.sort((a, b) => a.sortIndex - b.sortIndex);

		const start = (page - 1) * size;
		const end = start + size;
		const pagedMediaList = allLibraryMedia.slice(start, end);

		const navigationIndexes = allLibraryMedia.reduce<{ label: string; index: number }[]>((acc, item, idx) => {
			const label = (item.title?.[0]?.toUpperCase() ?? '#');
			const lastLabel = acc.at(-1)?.label;
			if (lastLabel !== label) {
				acc.push({ label, index: idx });
			}
			return acc;
		}, []);

		const totalMovieCount = requestedMediaType === PlexMediaType.Movie ? allLibraryMedia.length : 0;
		const totalTvShowCount = requestedMediaType === PlexMediaType.TvShow ? allLibraryMedia.length : 0;
		const totalSeasonCount = requestedMediaType === PlexMediaType.TvShow ? allLibraryMedia.reduce((acc, x) => acc + x.childCount, 0) : 0;
		const totalEpisodeCount = requestedMediaType === PlexMediaType.TvShow ? allLibraryMedia.reduce((acc, x) => acc + x.grandChildCount, 0) : 0;
		const totalMediaSize = allLibraryMedia.reduce((acc, x) => acc + x.mediaSize, 0);

		const response: PlexMediaStatisticsDTO = {
			countries: [],
			episodeCount: requestedMediaType === PlexMediaType.TvShow ? pagedMediaList.reduce((acc, x) => acc + x.grandChildCount, 0) : 0,
			genres: [],
			mediaCount: pagedMediaList.length,
			mediaList: pagedMediaList,
			mediaSize: pagedMediaList.reduce((acc, x) => acc + x.mediaSize, 0),
			movieCount: requestedMediaType === PlexMediaType.Movie ? pagedMediaList.length : 0,
			navigationIndexes,
			page,
			pageSize: size,
			qualities: [],
			queryHash: `library-${requestedLibraryId}-${requestedMediaType}`,
			roles: [],
			seasonCount: requestedMediaType === PlexMediaType.TvShow ? pagedMediaList.reduce((acc, x) => acc + x.childCount, 0) : 0,
			totalCount: allLibraryMedia.length,
			totalEpisodeCount,
			totalMediaSize,
			totalMovieCount,
			totalSeasonCount,
			totalTvShowCount,
			tvShowCount: requestedMediaType === PlexMediaType.TvShow ? pagedMediaList.length : 0,
		};

		if (requestedLibraryId !== 0 && (!library || !mediaData)) {
			req.reply({
				statusCode: 200,
				body: generateResultDTO({ ...response, mediaList: [], mediaCount: 0, totalCount: 0 }),
			});
			return;
		}

		req.reply({
			statusCode: 200,
			body: generateResultDTO(response),
		});
	});

	return this;
}
