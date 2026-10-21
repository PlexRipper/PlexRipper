import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import { flushPromises } from '@vue/test-utils';
import { baseSetup } from '@services-test-base';
import { generatePlexLibrary, generatePlexServer, Seed } from '@mock';
import { PlexMediaType } from '@dto';
import { useLibraryStore, useMediaOverviewStore, usePhotoBrowsingStore, useServerStore, useSettingsStore } from '@store';
import messages from '@/lang/en-US.json';
import MediaOverviewBarHeader from '@/components/MediaOverview/MediaOverviewBarHeader.vue';
import MediaOverviewTypeSelector from '@/components/MediaOverview/MediaOverviewTypeSelector.vue';

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
	test('delegates family switching through the extracted controlled selector', async () => {
		const settingsStore = useSettingsStore();
		settingsStore.displaySettings.allOverviewViewMode = PlexMediaType.Movie;
		useMediaOverviewStore().libraryId = 0;

		const wrapper = await mountSuspended(MediaOverviewBarHeader, {
			attachTo: document.body,
			props: { libraryId: 0 },
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QMediaTypeIcon: true },
			},
		});
		await wrapper.get('.media-overview-bar-header__item').trigger('click');
		await flushPromises();
		const selector = wrapper.findAllComponents(MediaOverviewTypeSelector)[0];

		expect(selector).toBeDefined();
		await selector!.get(`[data-cy="media-overview-type-${PlexMediaType.PhotoAlbum}"]`).trigger('click');

		expect(settingsStore.displaySettings.allOverviewViewMode).toBe(PlexMediaType.PhotoAlbum);
		wrapper.unmount();
	});
	test('keeps generic aggregate photo metadata sourced from overview totals', async () => {
		// Arrange
		const settingsStore = useSettingsStore();
		settingsStore.displaySettings.allOverviewViewMode = PlexMediaType.PhotoAlbum;
		const mediaOverviewStore = useMediaOverviewStore();
		mediaOverviewStore.libraryId = 0;
		mediaOverviewStore.totalCount = 17;
		mediaOverviewStore.allFileSize = 4096;

		// Act
		const wrapper = await mountSuspended(MediaOverviewBarHeader, {
			props: { libraryId: 0 },
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QMediaTypeIcon: true },
			},
		});

		// Assert
		expect(wrapper.text()).toContain('17 Albums');
		expect(wrapper.text()).not.toContain('0 Albums');
		wrapper.unmount();
	});
	test('uses the scoped photo library identity and persisted photo metadata in photo mode', async () => {
		// Arrange
		const library = generatePlexLibrary({
			seed: new Seed(1202),
			plexServerId: 9,
			type: PlexMediaType.PhotoAlbum,
			partialData: {
				id: 9,
				count: 4,
				photoAlbumCount: 4,
				photoImageCount: 5,
				photoClipCount: 1,
				mediaSize: 4096,
			},
		});
		useLibraryStore().updateLibrary(library);
		useServerStore().servers = [generatePlexServer({ id: 9, config: { seed: 1203 } })];
		const mediaOverviewStore = useMediaOverviewStore();
		mediaOverviewStore.totalCount = 99;
		mediaOverviewStore.allFileSize = 999999;

		// Act
		const wrapper = await mountSuspended(MediaOverviewBarHeader, {
			props: { libraryId: library.id, photoMode: true },
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QMediaTypeIcon: true },
			},
		});

		// Assert
		expect(wrapper.text()).toContain(library.title);
		expect(wrapper.text()).toContain('4 Albums');
		expect(wrapper.text()).toContain('5 Photos');
		expect(wrapper.text()).toContain('1 Clips');
		expect(wrapper.text()).not.toContain('99 Albums');
		expect(wrapper.findComponent(MediaOverviewTypeSelector).exists()).toBe(false);
		wrapper.unmount();
	});

	test.each(['albums', 'folders'] as const)('Should keep library counts visible in empty and refreshing %s views', async (view) => {
		// Arrange
		const library = generatePlexLibrary({
			seed: new Seed(1207),
			plexServerId: 9,
			type: PlexMediaType.PhotoAlbum,
			partialData: { id: 9, count: 0, photoAlbumCount: 0, photoImageCount: 17, photoClipCount: 1, mediaSize: 4096 },
		});
		useLibraryStore().updateLibrary(library);
		const photoStore = usePhotoBrowsingStore();
		photoStore.view = view;
		const wrapper = await mountSuspended(MediaOverviewBarHeader, {
			props: { libraryId: library.id, photoMode: true },
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QMediaTypeIcon: true },
			},
		});

		// Act
		expect(wrapper.text()).toContain('0 Albums - 17 Photos - 1 Clips - 4.1 kB');
		photoStore.loading = true;
		if (view === 'albums') photoStore.albums.loading = true;
		else photoStore.folders.loading = true;
		await flushPromises();

		// Assert
		expect(photoStore.isRefreshing).toBe(true);
		expect(wrapper.text()).toContain('0 Albums - 17 Photos - 1 Clips - 4.1 kB');
		useLibraryStore().updateLibrary({ ...library, photoImageCount: 18 });
		await flushPromises();
		expect(wrapper.text()).toContain('0 Albums - 18 Photos - 1 Clips - 4.1 kB');
		wrapper.unmount();
	});

	test('forwards aggregate photo family selection without changing generic overview settings', async () => {
		// Arrange
		const settingsStore = useSettingsStore();
		settingsStore.displaySettings.allOverviewViewMode = PlexMediaType.PhotoAlbum;

		// Act
		const wrapper = await mountSuspended(MediaOverviewBarHeader, {
			attachTo: document.body,
			props: { libraryId: 0, photoMode: true },
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QMediaTypeIcon: true },
			},
		});
		await wrapper.get('.media-overview-bar-header__item').trigger('click');
		await flushPromises();
		const selector = wrapper.findAllComponents(MediaOverviewTypeSelector)[0];
		await selector!.get(`[data-cy="media-overview-type-${PlexMediaType.Movie}"]`).trigger('click');

		// Assert
		expect(wrapper.emitted('family-change')).toEqual([[PlexMediaType.Movie]]);
		expect(settingsStore.displaySettings.allOverviewViewMode).toBe(PlexMediaType.PhotoAlbum);
		wrapper.unmount();
	});
});
