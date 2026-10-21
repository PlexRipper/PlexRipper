import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { defineComponent, h, nextTick, reactive, type Component } from 'vue';
import { createPinia, setActivePinia } from 'pinia';
import { mountSuspended, mockNuxtImport } from '@nuxt/test-utils/runtime';
import { flushPromises } from '@vue/test-utils';
import { createI18n } from 'vue-i18n';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { useGlobalStore, usePhotoBrowsingStore, useSettingsStore } from '@store';
import { ViewMode } from '@dto';
import { generateResultDTO } from '@mock';
import PhotoBrowser from '@/components/Photos/PhotoBrowser.vue';
import messages from '@/lang/en-US.json';

const navigation = vi.hoisted(() => ({
	route: { path: '/photos', fullPath: '/photos', query: {} as Record<string, string>, params: {} },
	push: vi.fn(), replace: vi.fn(), back: vi.fn(),
}));
mockNuxtImport('useRoute', () => () => navigation.route);
mockNuxtImport('useRouter', () => () => ({ push: navigation.push, replace: navigation.replace, back: navigation.back }));

describe('PhotoBrowser - unified route context', () => {
	let { mock } = baseVars();
	let pinia = createPinia();
	const mounted: Array<{ unmount: () => void }> = [];

	beforeAll(() => baseSetup());
	beforeEach(async () => {
		mock = getAxiosMock();
		pinia = createPinia();
		setActivePinia(pinia);
		navigation.route = reactive({ path: '/photos', fullPath: '/photos', query: {} as Record<string, string>, params: {} });
		navigation.push.mockReset().mockResolvedValue(undefined);
		navigation.replace.mockReset().mockResolvedValue(undefined);
		navigation.back.mockReset();
		await subscribeSpyTo(usePhotoBrowsingStore().setup()).onComplete();
		useGlobalStore().pageReadyObservable.next(false);
	});
	afterEach(() => {
		mounted.forEach((wrapper) => wrapper.unmount());
		mounted.length = 0;
		usePhotoBrowsingStore().$reset();
		mock.restore();
		vi.restoreAllMocks();
	});
	async function render(libraryId = 0, albumId?: number, overviewStub: boolean | Component = true) {
		useSettingsStore().displaySettings.photoAlbumViewMode = ViewMode.Table;
		const wrapper = await mountSuspended(PhotoBrowser, {
			props: { libraryId, albumId },
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: {
					MediaOverviewBar: true,
					PhotoOverviewList: overviewStub,
					DownloadConfirmation: true,
					RefreshModeDialog: true,
					PhotoAssetDialog: true,
					MediaOverviewRefresh: true,
					QLoadingOverlay: true,
				},
			},
		});
		mounted.push(wrapper);
		return wrapper;
	}

	test('Should activate and load the requested photo view when application setup becomes ready', async () => {
		// Arrange
		navigation.route.fullPath = '/photos?view=assets';
		navigation.route.query = { view: 'assets' };
		mock.onGet('/api/PlexPhoto/assets').reply(200, generateResultDTO({
			items: [], page: 1, pageSize: 100, totalCount: 0, totalBytes: 0,
		}));
		await render();
		const photoStore = usePhotoBrowsingStore();
		expect(photoStore.active).toBe(false);
		expect(mock.history.get.some((request) => request.url === '/api/PlexPhoto/assets')).toBe(false);

		// Act
		useGlobalStore().pageReadyObservable.next(true);
		await nextTick();
		await flushPromises();

		// Assert
		expect(photoStore.active).toBe(true);
		expect(photoStore.view).toBe('assets');
		expect(photoStore.assets.loaded).toBe(true);
		expect(photoStore.assets.error).toBe(false);
		expect(mock.history.get.filter((request) => request.url === '/api/PlexPhoto/assets')).toHaveLength(1);
	});

	test('Should accept a legacy page query while canonicalizing it away without rejecting its folder', async () => {
		// Arrange
		navigation.route.path = '/photos/4';
		navigation.route.fullPath = '/photos/4?view=folders&folderId=9&folderPage=2&assetPage=3';
		navigation.route.query = { view: 'folders', folderId: '9', folderPage: '2', assetPage: '3' };

		// Act
		const wrapper = await render(4);

		// Assert
		expect(wrapper.find('[data-cy="photo-invalid-route"]').exists()).toBe(false);
		expect(navigation.replace).toHaveBeenCalledWith({ query: { view: 'folders', folderId: '9' } });
	});

	test('Should restore focus through the rendered overview target after closing the viewer', async () => {
		// Arrange
		const focusItem = vi.fn(async () => true);
		const focusFallback = vi.fn();
		const overviewStub = defineComponent({
			name: 'PhotoOverviewList',
			setup(_, { expose }) {
				expose({ focusItem, focusFallback });
				return () => h('section', { 'data-cy': 'photo-overview-list' });
			},
		});
		await render(4, undefined, overviewStub);
		const photoStore = usePhotoBrowsingStore();

		// Act
		photoStore.focusIndex = 7;
		await nextTick();
		await flushPromises();

		// Assert
		expect(focusItem).toHaveBeenCalledWith(7);
		expect(focusFallback).not.toHaveBeenCalled();
		expect(photoStore.focusIndex).toBeNull();
	});
});
