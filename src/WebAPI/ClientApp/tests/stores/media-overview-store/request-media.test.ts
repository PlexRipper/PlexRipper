import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	generatePlexMediaSlims,
	generatePlexMediaStatisticsDTO,
	generateResultDTO,
} from '@mock';
import { useMediaOverviewStore } from '@store';
import { PlexMediaType } from '@dto';

describe('MediaOverviewStore.requestMedia()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should set media in MediaOverviewStore when media is retrieved with success', async () => {
		// Arrange
		const mediaOverviewStore = useMediaOverviewStore();
		const type = PlexMediaType.Movie;
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: {
				movieCount: 100,
			},
			partialData: {
				plexServerId: 1,
				plexLibraryId: 1,
				type,
			},
		}));

		const url = new RegExp(`/api/PlexMedia/*`);
		mock.onGet(url).reply(200, generateResultDTO(movies));

		// Act
		const result = subscribeSpyTo(mediaOverviewStore.requestMedia());
		await result.onComplete();

		// Assert
		expect(result.receivedComplete()).toEqual(true);
		expect(mediaOverviewStore.scrollDict).not.deep.equal({ '#': 0 });
		expect(mediaOverviewStore.scrollAlphabet.length).to.be.greaterThanOrEqual(5);
		expect(mediaOverviewStore.allMovieCount).toEqual(movies.movieCount);
		expect(mediaOverviewStore.allTvShowCount).toEqual(movies.tvShowCount);
		expect(mediaOverviewStore.allSeasonCount).toEqual(movies.seasonCount);
		expect(mediaOverviewStore.allEpisodeCount).toEqual(movies.episodeCount);
		expect(mediaOverviewStore.allFileSize).toEqual(movies.mediaSize);
		expect(mediaOverviewStore.loading).toEqual(false);
	});
});
