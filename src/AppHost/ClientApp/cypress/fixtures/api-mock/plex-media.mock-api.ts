import { type BasePageSetupResult, headers, urlBuilder } from '@fixtures';
import type { MockConfig } from '@mock';
import { generatePlexMedia, generatePlexMediaSlims, generateResultDTO } from '@mock';
import { PlexLibraryPaths, PlexMediaPaths } from '@api/api-paths';
import { type PlexMediaStatisticsDTO, PlexMediaType, VideoQuality } from '@dto';

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

		// Generate media statistics
		const mediaStatisticsDTO: PlexMediaStatisticsDTO = {
			movieCount: library.type === PlexMediaType.Movie ? mediaList.length : 0,
			tvShowCount: library.type === PlexMediaType.TvShow ? mediaList.length : 0,
			seasonCount: library.type === PlexMediaType.TvShow ? mediaList.reduce((acc, x) => acc + x.childCount, 0) : 0,
			episodeCount: library.type === PlexMediaType.TvShow ? mediaList.reduce((acc, x) => acc + x.grandChildCount, 0) : 0,
			mediaSize: mediaList.reduce((acc, x) => acc + x.mediaSize, 0),
			mediaCount: mediaList.length,
			mediaList,
		};

		// Library media endpoints
		const url = urlBuilder(PlexLibraryPaths.getPlexLibraryMediaEndpoint(library.id, {
			countryId: 0,
			filterOfflineMedia: false,
			filterOwnedMedia: false,
			genreId: 0,
			roleId: 0,
			page: 0,
			size: 0,
			quality: VideoQuality.None,
		}));

		cy.intercept(
			'GET',
			url.pathname + '*',
			{
				statusCode: 200,
				body: generateResultDTO(mediaStatisticsDTO),
				...headers,
			},
		);

		for (const mediaItem of mediaList) {
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

	for (const mediaType of [PlexMediaType.Movie, PlexMediaType.TvShow]) {
		cy.intercept(
			'GET', '/api/PlexMedia*',
			{
				statusCode: 200,
				body: generateResultDTO(
					this.mediaData.filter((x) => x.media.some((y) => y.type === mediaType)).flatMap((x) => x.media),
				),
				...headers,
			},
		);
	}

	return this;
}
