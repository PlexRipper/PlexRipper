import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import { baseSetup } from '@services-test-base';
import { generatePlexLibrary, Seed } from '@mock';
import { PlexMediaType } from '@dto';
import { useLibraryStore, useMediaOverviewStore } from '@store';
import messages from '@/lang/en-US.json';
import MediaOverviewBarHeader from '@/components/MediaOverview/MediaOverviewBarHeader.vue';

describe('MediaOverviewBarHeader - library media counts', () => {
	let pinia = createPinia();

	beforeAll(() => baseSetup());
	beforeEach(() => {
		pinia = createPinia();
		setActivePinia(pinia);
	});

	test.each([
		{
			type: PlexMediaType.MusicArtist,
			count: 2,
			partialData: { musicArtistCount: 2, musicAlbumCount: 3, musicTrackCount: 7 },
			expected: ['2 Artists', '3 Albums', '7 Tracks'],
		},
		{
			type: PlexMediaType.PhotoAlbum,
			count: 4,
			partialData: { photoAlbumCount: 4, photoImageCount: 5, photoClipCount: 1 },
			expected: ['4 Albums', '5 Photos', '1 Clips'],
		},
		{
			type: PlexMediaType.OtherVideos,
			count: 6,
			partialData: { otherVideoCount: 6 },
			expected: ['6 Videos'],
		},
	])('Should render persisted counts for a $type library', async ({ type, count, partialData, expected }) => {
		// Arrange
		const library = generatePlexLibrary({
			seed: new Seed(1201),
			plexServerId: 1,
			type,
			partialData: { id: 1, count, ...partialData },
		});
		const libraryStore = useLibraryStore();
		const mediaOverviewStore = useMediaOverviewStore();
		libraryStore.updateLibrary(library);
		mediaOverviewStore.libraryId = library.id;
		mediaOverviewStore.totalCount = library.count;

		// Act
		const wrapper = await mountSuspended(MediaOverviewBarHeader, {
			props: { libraryId: library.id },
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QMediaTypeIcon: true },
			},
		});

		// Assert
		for (const value of expected) {
			expect(wrapper.text()).toContain(value);
		}
		wrapper.unmount();
	});
});
