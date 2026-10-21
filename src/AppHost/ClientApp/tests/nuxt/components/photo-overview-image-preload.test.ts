import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { nextTick } from 'vue';
import { createI18n } from 'vue-i18n';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import type { VueWrapper } from '@vue/test-utils';
import { baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { PhotoAssetKind, PhotoItemKind, type PhotoAlbumBrowseDTO, type PhotoAssetBrowseDTO, type PhotoFolderBrowseDTO } from '@dto';
import { usePhotoBrowsingStore } from '@store';
import type { PhotoOverviewItem } from '@/store/photoBrowsingStore';
import PhotoOverviewPosterCard from '@/components/Photos/PhotoOverviewPosterCard.vue';
import PhotoOverviewTableRow from '@/components/Photos/PhotoOverviewTableRow.vue';
import messages from '@/lang/en-US.json';

type ObserverCallback = (entries: IntersectionObserverEntry[], observer: IntersectionObserver) => void;

class TestIntersectionObserver {
	static instances: TestIntersectionObserver[] = [];
	private readonly callback: ObserverCallback;

	constructor(callback: ObserverCallback) {
		this.callback = callback;
		TestIntersectionObserver.instances.push(this);
	}

	observe(): void {}
	unobserve(): void {}
	disconnect(): void {}
	takeRecords(): IntersectionObserverEntry[] { return []; }

	trigger(isIntersecting: boolean): void {
		this.callback([{ isIntersecting, time: 1 } as IntersectionObserverEntry], this as unknown as IntersectionObserver);
	}
}

const asset = (id: number, assetKind = PhotoAssetKind.Image): PhotoAssetBrowseDTO => ({
	id,
	plexLibraryId: 4,
	plexServerId: 3,
	title: `Asset ${id}`,
	plexServerName: 'Camera server',
	plexLibraryTitle: 'Travel photos',
	assetKind,
	hasThumbnail: false,
	plexApiRatingKey: id,
	plexApiMetaDataKey: id,
	originalFilename: `${id}.${assetKind === PhotoAssetKind.VideoClip ? 'mp4' : 'jpg'}`,
	container: assetKind === PhotoAssetKind.VideoClip ? 'mp4' : 'jpg',
	size: 2048,
	width: 1920,
	height: 1080,
	duration: null,
	relativeFolderPath: '',
	sourceRootTitle: 'Camera',
});

const folder: PhotoFolderBrowseDTO = {
	id: 8,
	plexLibraryId: 4,
	plexServerId: 3,
	title: 'Summer',
	relativePath: 'Summer',
	parentId: null,
	isRoot: false,
	isActive: null,
	recursiveAssetCount: 2,
	recursiveAssetBytes: 4096,
};

const album: PhotoAlbumBrowseDTO = {
	id: 9,
	plexLibraryId: 4,
	plexServerId: 3,
	title: 'Highlights',
	plexServerName: 'Camera server',
	plexLibraryTitle: 'Travel photos',
	assetCount: 2,
	assetBytes: 4096,
	hasThumbnail: false,
	plexApiRatingKey: 9,
	plexApiMetaDataKey: 9,
};

const i18n = () => createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } });
const posterStubs = {
	MediaPosterCard: {
		inheritAttrs: false,
		template: '<div v-bind="$attrs"><slot name="image" /><slot name="metadata" :compact="false" /><slot name="actions" :compact="false" :activate="() => undefined" /></div>',
	},
	QGlowContainer: { template: '<div><slot /></div>' },
	QText: { props: { value: { type: String, default: '' } }, template: '<span>{{ value }}</span>' },
	QMediaTypeIcon: true,
	BaseButton: true,
	PhotoThumbnail: true,
};

let pinia = createPinia();
let { mock } = baseVars();
let mounted: VueWrapper[] = [];

beforeAll(() => baseSetup());
beforeEach(() => {
	mock = getAxiosMock();
	pinia = createPinia();
	setActivePinia(pinia);
	TestIntersectionObserver.instances = [];
	vi.stubGlobal('IntersectionObserver', TestIntersectionObserver);
});
afterEach(() => {
	for (const wrapper of mounted)
		wrapper.unmount();
	mounted = [];
	mock.restore();
	vi.unstubAllGlobals();
	vi.restoreAllMocks();
});

function observer(): TestIntersectionObserver {
	const instance = TestIntersectionObserver.instances[TestIntersectionObserver.instances.length - 1];
	if (!instance)
		throw new Error('Expected a visibility observer');
	return instance;
}

describe('Photo overview fullscreen image preload candidates', () => {
	test('queues visible poster images and cleans changed, offscreen, and unmounted candidates', async () => {
		// Arrange
		const photoStore = usePhotoBrowsingStore();
		const setCandidate = vi.spyOn(photoStore, 'setImagePreloadCandidate');
		const first = asset(10);
		const wrapper = await mountSuspended(PhotoOverviewPosterCard, {
			props: { item: { kind: PhotoItemKind.Asset, asset: first, index: 0 } },
			global: { plugins: [pinia, i18n()], stubs: posterStubs },
		});
		mounted.push(wrapper);
		await nextTick();

		// Act
		observer().trigger(true);
		await nextTick();

		// Assert
		const owner = setCandidate.mock.calls[0]?.[2];
		expect(owner).toBeDefined();
		expect(setCandidate).toHaveBeenCalledWith(first, true, owner);

		// Act
		const refreshed = { ...first, title: 'Asset 10 refreshed' };
		await wrapper.setProps({ item: { kind: PhotoItemKind.Asset, asset: refreshed, index: 0 } });

		// Assert
		expect(setCandidate).toHaveBeenNthCalledWith(2, first, false, owner);
		expect(setCandidate).toHaveBeenNthCalledWith(3, refreshed, true, owner);

		// Act
		const second = asset(11);
		await wrapper.setProps({ item: { kind: PhotoItemKind.Asset, asset: second, index: 1 } });

		// Assert
		expect(setCandidate).toHaveBeenNthCalledWith(4, refreshed, false, owner);
		expect(setCandidate).toHaveBeenNthCalledWith(5, second, true, owner);

		// Act
		observer().trigger(false);
		await nextTick();

		// Assert
		expect(setCandidate).toHaveBeenLastCalledWith(second, false, owner);

		// Act
		observer().trigger(true);
		await nextTick();
		wrapper.unmount();

		// Assert
		expect(setCandidate).toHaveBeenLastCalledWith(second, false, owner);
		expect(setCandidate.mock.calls.every((call) => call[2] === owner)).toBe(true);
	});

	test('queues visible table images and removes them when they leave the viewport', async () => {
		// Arrange
		const photoStore = usePhotoBrowsingStore();
		const setCandidate = vi.spyOn(photoStore, 'setImagePreloadCandidate');
		const photo = asset(12);
		const wrapper = await mountSuspended(PhotoOverviewTableRow, {
			props: { item: { kind: PhotoItemKind.Asset, asset: photo, index: 0 } },
			global: { plugins: [pinia, i18n()], stubs: { QFileSize: true } },
		});
		mounted.push(wrapper);
		await nextTick();

		// Act
		observer().trigger(true);
		await nextTick();

		// Assert
		const owner = setCandidate.mock.calls[0]?.[2];
		expect(owner).toBeDefined();
		expect(setCandidate).toHaveBeenCalledWith(photo, true, owner);

		// Act
		observer().trigger(false);
		await nextTick();
		wrapper.unmount();

		// Assert
		expect(setCandidate).toHaveBeenLastCalledWith(photo, false, owner);
	});

	test.each([
		['folders', { kind: PhotoItemKind.Folder, folder, index: 0 } as PhotoOverviewItem],
		['albums', { kind: PhotoItemKind.Album, album, index: 0 } as PhotoOverviewItem],
		['video clips', { kind: PhotoItemKind.Asset, asset: asset(13, PhotoAssetKind.VideoClip), index: 0 } as PhotoOverviewItem],
	])('does not register %s for poster or table rows', async (_label, item) => {
		// Arrange
		const photoStore = usePhotoBrowsingStore();
		const setCandidate = vi.spyOn(photoStore, 'setImagePreloadCandidate');
		const poster = await mountSuspended(PhotoOverviewPosterCard, {
			props: { item },
			global: { plugins: [pinia, i18n()], stubs: posterStubs },
		});
		mounted.push(poster);
		await nextTick();

		// Act
		observer().trigger(true);
		await nextTick();

		// Assert
		expect(setCandidate).not.toHaveBeenCalled();

		// Act
		const table = await mountSuspended(PhotoOverviewTableRow, {
			props: { item },
			global: { plugins: [pinia, i18n()], stubs: { QFileSize: true } },
		});
		mounted.push(table);
		await nextTick();
		observer().trigger(true);
		await nextTick();

		// Assert
		expect(setCandidate).not.toHaveBeenCalled();
	});
});
