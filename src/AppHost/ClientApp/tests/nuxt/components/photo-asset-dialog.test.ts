import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import type { Mock } from 'vitest';
import { defineComponent, h, nextTick, reactive, type VNode } from 'vue';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import { of, type Observable } from 'rxjs';
import { baseSetup } from '@services-test-base';
import { PhotoAssetKind, type PhotoAssetBrowseDTO, type PhotoAssetInspectionDTO } from '@dto';
import messages from '@/lang/en-US.json';
import PhotoAssetDialog from '@/components/Photos/PhotoAssetDialog.vue';

const usePhotoBrowsingStoreMock = vi.hoisted(() => vi.fn());
vi.mock('@store', () => ({ usePhotoBrowsingStore: usePhotoBrowsingStoreMock }));

type TestSlide = {
	asset: PhotoAssetBrowseDTO;
	index: number;
	url: string;
	representation: 'original' | 'converted' | 'preview';
	loading: boolean;
	error: boolean;
};

interface ViewerStore {
	dialog: {
		open: boolean;
		loading: boolean;
		error: boolean;
		asset: PhotoAssetInspectionDTO | null;
	};
	current: PhotoAssetBrowseDTO | null;
	index: number;
	count: number;
	detailsOpen: boolean;
	zoomed: boolean;
	navigating: boolean;
	slides: TestSlide[];
	showAsset: Mock<(index: number) => Observable<void>>;
	closeAssetDialog: Mock<() => void>;
	openDetails: Mock<() => Observable<void>>;
	closeDetails: Mock<() => void>;
	setZoomed: Mock<(zoomed: boolean) => void>;
	retryImage: Mock<() => Observable<void>>;
	imageFailed: Mock<(index: number) => Observable<void>>;
	retryInspection: Mock<() => Observable<void>>;
	requestInspectionMembershipPage: Mock<(page: number) => Observable<void>>;
}

describe('PhotoAssetDialog - image-first viewer', () => {
	let store: ViewerStore;
	const mounted: Array<{ unmount: () => void }> = [];

	beforeAll(() => baseSetup());
	beforeEach(() => {
		store = createViewerStore();
		usePhotoBrowsingStoreMock.mockReturnValue(store);
	});
	afterEach(() => {
		mounted.forEach((wrapper) => wrapper.unmount());
		mounted.length = 0;
		vi.clearAllMocks();
	});

	function browseAsset(index: number, kind = PhotoAssetKind.Image): PhotoAssetBrowseDTO {
		return {
			id: index + 10,
			plexServerId: 1,
			plexLibraryId: 5,
			plexServerName: 'Holiday server',
			plexLibraryTitle: 'Family photos',
			title: `Asset ${index}`,
			assetKind: kind,
			hasThumbnail: true,
			plexApiRatingKey: index + 10,
			plexApiMetaDataKey: index + 10,
			originalFilename: `asset-${index}.jpg`,
			size: 2048,
			container: 'jpg',
			width: 1920,
			height: 1080,
			duration: kind === PhotoAssetKind.VideoClip ? 6 : null,
			relativeFolderPath: 'Trips/Summer',
			sourceRootTitle: 'Camera',
		};
	}

	function inspection(): PhotoAssetInspectionDTO {
		return {
			...browseAsset(0),
			addedAt: '2026-01-02T00:00:00Z',
			updatedAt: null,
			originallyAvailableAt: null,
			summary: 'A summer photo',
			videoCodec: null,
			audioCodec: null,
			breadcrumbs: [
				{ id: 3, title: 'Camera', relativePath: '' },
				{ id: 4, title: 'Trips', relativePath: 'Trips' },
			],
			albums: {
				items: [{
					id: 1,
					plexServerId: 1,
					plexLibraryId: 5,
					plexServerName: 'Holiday server',
					plexLibraryTitle: 'Family photos',
					title: 'Summer album',
					hasThumbnail: true,
					plexApiRatingKey: 11,
					plexApiMetaDataKey: 11,
				}],
				page: 1,
				pageSize: 1,
				totalCount: 2,
			},
		};
	}

	function createViewerStore(): ViewerStore {
		const assets = Array.from({ length: 5 }, (_, index) => browseAsset(index));
		const slides = assets.slice(0, 3).map((asset, index) => ({
			asset,
			index,
			url: `blob:asset-${index}`,
			representation: 'original' as const,
			loading: false,
			error: false,
		}));
		let viewer!: ViewerStore;
		const raw = {
			dialog: reactive({
				open: true,
				loading: false,
				error: false,
				asset: null as PhotoAssetInspectionDTO | null,
			}),
			current: assets[0],
			index: 0,
			count: assets.length,
			detailsOpen: false,
			zoomed: false,
			navigating: false,
			slides,
			showAsset: vi.fn(() => of(undefined)),
			openDetails: vi.fn(() => {
				viewer.detailsOpen = true;
				viewer.dialog.asset = inspection();
				return of(undefined);
			}),
			setZoomed: vi.fn((zoomed: boolean) => { viewer.zoomed = zoomed; }),
			retryImage: vi.fn(() => of(undefined)),
			imageFailed: vi.fn(() => of(undefined)),
			retryInspection: vi.fn(() => of(undefined)),
			requestInspectionMembershipPage: vi.fn(() => of(undefined)),
		};
		viewer = reactive(raw) as unknown as ViewerStore;
		return viewer;
	}

	const stubs = {
		QDialog: { props: ['modelValue'], template: '<div v-if="modelValue"><slot /></div>' },
		QCard: { template: '<div><slot /></div>' },
		QCardSection: { template: '<div><slot /></div>' },
		QSeparator: { template: '<hr />' },
		QCarousel: defineComponent({
			name: 'QCarousel',
			props: { modelValue: { type: [String, Number], default: '' } },
			setup(props, { slots }) {
				return () => {
					const slides = slots.default?.() ?? [];
					const activeSlide = slides.find((vnode) => String((vnode as VNode).props?.name) === String(props.modelValue)) ?? slides[0];
					return h('div', { class: 'q-carousel' }, [
						activeSlide,
						...(slots.control?.() ?? []),
					]);
				};
			},
		}),
		QCarouselSlide: { props: ['name'], template: '<div class="q-carousel-slide" :data-slide="name"><slot /></div>' },
		QVirtualScroll: {
			name: 'QVirtualScroll',
			props: ['items', 'virtualScrollItemSize'],
			emits: ['virtual-scroll'],
			template: '<div><slot v-for="(item, index) in items" :item="item" :index="index" /></div>',
		},
		QBtn: {
			props: ['label', 'ariaLabel'],
			emits: ['click'],
			template: '<button :aria-label="ariaLabel" @click="$emit(\'click\')"><slot />{{ label }}</button>',
		},
		QMenu: { template: '<div class="q-menu"><slot /></div>' },
		QList: { template: '<div><slot /></div>' },
		QItem: { emits: ['click'], template: '<div @click="$emit(\'click\')"><slot /></div>' },
		QItemSection: { template: '<div><slot /></div>' },
		QSpinner: { template: '<span role="status" />' },
		QIcon: { template: '<span />' },
		QChip: { props: ['label'], template: '<span><slot />{{ label }}</span>' },
		QAlert: { template: '<div><slot /></div>' },
		QBanner: { template: '<div><slot /><slot name="action" /></div>' },
		QBreadcrumbs: { template: '<nav><slot /></nav>' },
		QBreadcrumbsEl: { props: ['label', 'to'], template: '<a :href="to"><slot />{{ label }}</a>' },
		QFileSize: { props: ['size'], template: '<span>{{ size }}</span>' },
		QDuration: { props: ['value'], template: '<span>{{ value }}</span>' },
		QDateTime: { props: ['text'], template: '<time>{{ text }}</time>' },
	};

	async function render() {
		const wrapper = await mountSuspended(PhotoAssetDialog, {
			global: {
				plugins: [createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs,
			},
		});
		mounted.push(wrapper);
		return wrapper;
	}

	test('Should show only the active image without loading inspection details', async () => {
		// Arrange
		const wrapper = await render();
		// Assert
		expect(wrapper.get('[data-cy="photo-viewer-image"]').attributes('src')).toBe('blob:asset-0');
		expect(wrapper.findAll('[data-cy="photo-viewer-image"]')).toHaveLength(1);
		expect(wrapper.find('h2').exists()).toBe(false);
		expect(wrapper.get('[data-cy="photo-asset-dialog"]').attributes('aria-label')).toBe('Asset 0');
		expect(wrapper.find('[data-cy="photo-viewer-details"]').exists()).toBe(false);
		expect(store.openDetails).not.toHaveBeenCalled();
		expect(store.dialog.asset).toBeNull();
	});

	test('Should dispatch full-sequence navigation beyond the three-slide window and wrap at the sequence edge', async () => {
		// Arrange
		const wrapper = await render();
		// Act
		await wrapper.get('[data-cy="photo-viewer-previous"]').trigger('click');
		await wrapper.get('[data-cy="photo-viewer-next"]').trigger('click');
		window.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowLeft' }));
		// Assert
		expect(store.showAsset).toHaveBeenNthCalledWith(1, 4);
		expect(store.showAsset).toHaveBeenNthCalledWith(2, 1);
		expect(store.showAsset).toHaveBeenNthCalledWith(3, 4);
	});

	test('Should load details only from the ellipsis action and render bounded memberships with real breadcrumbs', async () => {
		// Arrange
		const wrapper = await render();
		// Act
		await wrapper.get('[data-cy="photo-viewer-details-action"]').trigger('click');
		const memberships = wrapper.findComponent({ name: 'QVirtualScroll' });
		expect(memberships.exists()).toBe(true);
		memberships.vm.$emit('virtual-scroll', { to: 0 });
		// Assert
		expect(store.openDetails).toHaveBeenCalledTimes(1);
		expect(wrapper.get('[data-cy="photo-viewer-details"]').text()).toContain('Summer album');
		expect(wrapper.findAll('a[href*="folderId="]')).toHaveLength(1);
		expect(wrapper.findComponent({ name: 'QPagination' }).exists()).toBe(false);
		expect(store.requestInspectionMembershipPage).toHaveBeenCalledWith(2);
	});

	test('Should keep clip thumbnails usable without claiming an original image failed or offering playback', async () => {
		// Arrange
		const clip = browseAsset(0, PhotoAssetKind.VideoClip);
		store.current = clip;
		store.slides = [{
			asset: clip,
			index: 0,
			url: 'blob:preview',
			representation: 'preview',
			loading: false,
			error: false,
		}];
		store.count = 1;
		const wrapper = await render();
		// A clip thumbnail is intentional, not a failed original image.
		expect(wrapper.find('[data-cy="photo-viewer-preview"]').exists()).toBe(false);
		// Act
		await wrapper.get('[data-cy="photo-viewer-image"]').trigger('error');
		store.slides[0]!.url = '';
		store.slides[0]!.error = true;
		await nextTick();
		await wrapper.get('[data-cy="photo-image-retry"]').trigger('click');
		expect(wrapper.find('[data-cy="photo-viewer-zoom"]').exists()).toBe(false);
		expect(wrapper.find('video, audio, [aria-label="Play"]').exists()).toBe(false);
		expect(store.imageFailed).toHaveBeenCalledWith(0);
		expect(store.retryImage).toHaveBeenCalledTimes(1);
	});

	test('Should label only an available fallback preview rather than a pending original request', async () => {
		// Arrange
		store.slides[0]!.url = '';
		store.slides[0]!.representation = 'preview';
		store.slides[0]!.loading = true;
		const wrapper = await render();
		expect(wrapper.find('[data-cy="photo-viewer-preview"]').exists()).toBe(false);

		// Act
		store.slides[0]!.url = 'blob:fallback';
		store.slides[0]!.loading = false;
		await nextTick();

		// Assert
		expect(wrapper.get('[data-cy="photo-viewer-image"]').attributes('src')).toBe('blob:fallback');
		expect(wrapper.get('[data-cy="photo-viewer-preview"]').text()).toContain('Preview — original unavailable');
	});

	test('Should switch the native-size control for still images', async () => {
		// Arrange
		const wrapper = await render();
		// Act
		await wrapper.get('[data-cy="photo-viewer-zoom"]').trigger('click');
		store.index = 1;
		store.current = store.slides[1]!.asset;
		await nextTick();
		// Assert
		expect(store.setZoomed).toHaveBeenCalledWith(true);
		expect(wrapper.get('[data-cy="photo-viewer-zoom"]').attributes('aria-label')).toBe('Fit image to window');
		expect(wrapper.get('[data-cy="photo-viewer-zoom"]').attributes('aria-pressed')).toBe('true');
	});
	test('Should pan a native-size image with a desktop pointer drag while zoomed', async () => {
		// Arrange
		const wrapper = await render();
		const surface = wrapper.get('.photo-asset-dialog__surface');
		Object.defineProperties(surface.element, {
			setPointerCapture: { configurable: true, value: vi.fn() },
			hasPointerCapture: { configurable: true, value: vi.fn(() => false) },
			releasePointerCapture: { configurable: true, value: vi.fn() },
		});
		await wrapper.get('[data-cy="photo-viewer-zoom"]').trigger('click');
		const image = wrapper.get('[data-cy="photo-viewer-image"]');
		// Act
		await image.trigger('pointerdown', { pointerId: 1, pointerType: 'mouse', button: 0, clientX: 100, clientY: 100 });
		await image.trigger('pointermove', { pointerId: 1, pointerType: 'mouse', buttons: 1, clientX: 140, clientY: 130 });
		await image.trigger('pointerup', { pointerId: 1, pointerType: 'mouse', button: 0 });
		// Assert
		expect(image.attributes('style')).toContain('translate3d(40px, 30px, 0)');
	});

	test('Should keep cached metadata visible when inspection reload fails and offer a retry', async () => {
		// Arrange
		const wrapper = await render();
		store.dialog.asset = inspection();
		store.detailsOpen = true;
		store.dialog.error = true;
		await nextTick();
		// Act
		await wrapper.get('[data-cy="photo-inspection-retry"]').trigger('click');
		// Assert
		expect(wrapper.get('[data-cy="photo-viewer-details"]').text()).toContain('A summer photo');
		expect(wrapper.text()).toContain('The latest asset details could not be loaded');
		expect(store.retryInspection).toHaveBeenCalledTimes(1);
	});
});
