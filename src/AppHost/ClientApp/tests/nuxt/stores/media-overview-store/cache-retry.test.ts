import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	generatePlexMediaSlims,
	generatePlexMediaStatisticsDTO,
	generateResultDTO,
} from '@mock';
import { useMediaOverviewStore } from '@store';
import { PlexMediaType } from '@dto';

interface MockResponse {
	isSuccess: boolean;
	statusCode: number;
	errors: Array<{ message: string; reasons: never[]; metadata: Record<string, never> }>;
	successes: never[];
	value: null;
}

describe('MediaOverviewStore - Cache Retry', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
		vi.useFakeTimers({ shouldAdvanceTime: false, toFake: ['setInterval', 'clearInterval', 'Date'] });
	});

	afterEach(() => {
		vi.useRealTimers();
	});

	function setupFilterMocks() {
		mock.onGet(new RegExp('/api/PlexLibrary/0/metadata-filter')).reply(200, generateResultDTO({
			countries: [],
			genres: [],
			qualities: [],
			roles: [],
		}));
		mock.onGet(new RegExp('/api/PlexLibrary/0/metadata\\?')).reply(200, generateResultDTO({
			episodeCount: 0, mediaCount: 0, mediaSize: 0, movieCount: 0, seasonCount: 0, tvShowCount: 0,
			mediaList: [], roles: [], countries: [], genres: [], qualities: [],
			roleCount: 0, countryCount: 0, genreCount: 0, qualityCount: 0,
		}));
	}

	function mediaFailureResponse(): [number, MockResponse] {
		return [200, {
			isSuccess: false, statusCode: 503,
			errors: [{ message: 'Cache warming up', reasons: [], metadata: {} }],
			successes: [], value: null,
		}];
	}

	function mediaSuccessResponse(): [number, ReturnType<typeof generateResultDTO>] {
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 5 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		return [200, generateResultDTO(movies)];
	}

	/** Advance through one full retry cycle: 5 interval ticks → refresh fires */
	async function advanceRetryCycle() {
		await vi.advanceTimersByTimeAsync(5000);
	}

	test('should auto-retry with countdown when cache is warming up', async () => {
		let callCount = 0;
		mock.onGet(new RegExp('/api/PlexMedia')).reply(() => {
			callCount++;
			if (callCount <= 2) return mediaFailureResponse();
			return mediaSuccessResponse();
		});
		setupFilterMocks();

		const store = useMediaOverviewStore();
		subscribeSpyTo(store.refreshMediaData());

		await vi.advanceTimersByTimeAsync(1000);
		expect(callCount).toBe(1);
		expect(store.serverError).toBe(true);
		expect(store.cacheRetrySeconds).toBe(4);

		await vi.advanceTimersByTimeAsync(4000);
		expect(callCount).toBe(2);
		expect(store.serverError).toBe(true);

		await advanceRetryCycle();
		expect(callCount).toBe(3);
		expect(store.serverError).toBe(false);
		expect(store.itemsLength).toBe(5);
	});

	test('should retry N times until cache is ready', async () => {
		let callCount = 0;
		mock.onGet(new RegExp('/api/PlexMedia')).reply(() => {
			callCount++;
			if (callCount <= 3) return mediaFailureResponse();
			return mediaSuccessResponse();
		});
		setupFilterMocks();

		const store = useMediaOverviewStore();
		subscribeSpyTo(store.refreshMediaData());
		await vi.advanceTimersByTimeAsync(1000);
		expect(callCount).toBe(1);
		expect(store.serverError).toBe(true);

		await vi.advanceTimersByTimeAsync(4000);
		expect(callCount).toBe(2);
		expect(store.serverError).toBe(true);

		await advanceRetryCycle();
		expect(callCount).toBe(3);
		expect(store.serverError).toBe(true);

		await advanceRetryCycle();
		expect(callCount).toBe(4);
		expect(store.serverError).toBe(false);
		expect(store.itemsLength).toBe(5);
	});

	test('refreshMediaData called manually while retrying should clear the retry', async () => {
		mock.onGet(new RegExp('/api/PlexMedia')).reply(() => mediaFailureResponse());
		setupFilterMocks();

		const store = useMediaOverviewStore();
		subscribeSpyTo(store.refreshMediaData());
		await vi.advanceTimersByTimeAsync(1000);
		expect(store.serverError).toBe(true);
		expect(store.cacheRetrySeconds).toBe(4);

		store.refreshMediaData().subscribe();
		expect(store.cacheRetrySeconds).toBe(0);
	});
});
