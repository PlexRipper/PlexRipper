import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup } from '@services-test-base';
import { useMediaOverviewStore } from '@store';
import { VideoQuality } from '@dto';

describe('MediaOverviewStore.buildFlexQueryParams()', () => {
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		setActivePinia(createPinia());
	});

	test('Should combine metadata and quality filters with ampersand separators', () => {
		// Arrange
		const mediaOverviewStore = useMediaOverviewStore();
		mediaOverviewStore.libraryId = 17;
		mediaOverviewStore.metadata = {
			countryId: 7,
			roleId: 11,
			genreId: 13,
			quality: VideoQuality.SD,
		};

		// Act
		const result = mediaOverviewStore.buildFlexQueryParams(1, 100);

		// Assert
		expect(result.plexLibraryId).toBe(17);
		expect(result.filter).toBe('Countries:any:Id:eq:7&Actors:any:Id:eq:11&Genres:any:Id:eq:13&quality:eq:SD');
	});
});
