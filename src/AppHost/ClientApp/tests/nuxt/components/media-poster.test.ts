import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { defineComponent, h, nextTick, onMounted, ref } from 'vue';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import { baseSetup } from '@services-test-base';
import { generatePlexMedia } from '@mock';
import { PlexMediaType } from '@dto';
import { useMediaOverviewStore } from '@store';
import MediaPoster from '@/components/MediaOverview/PosterTable/MediaPoster.vue';
import MediaPosterCard from '@/components/MediaOverview/PosterTable/MediaPosterCard.vue';
import PosterTable from '@/components/MediaOverview/PosterTable/PosterTable.vue';
import messages from '@/lang/en-US.json';

const glowContainerStub = defineComponent({
	name: 'QGlowContainer',
	setup(_, { slots }) {
		return () => h('div', { class: 'glow-container-stub' }, slots.default?.());
	},
});

const textStub = defineComponent({
	name: 'QText',
	props: { value: { type: String, default: '' } },
	template: '<span class="text-stub">{{ value }}</span>',
});

const mediaTypeIconStub = defineComponent({
	name: 'QMediaTypeIcon',
	template: '<span class="media-type-icon-stub" />',
});

const baseButtonStub = defineComponent({
	name: 'BaseButton',
	emits: ['click'],
	template: '<button type="button" @click="$emit(\'click\')">Action</button>',
});

const loadingOverlayStub = defineComponent({
	name: 'QLoadingOverlay',
	template: '<div class="loading-overlay-stub" />',
});

const mediaQualityStub = defineComponent({
	name: 'MediaQuality',
	template: '<div class="media-quality-stub" />',
});

const comparisonStateButtonStub = defineComponent({
	name: 'MediaComparisonStateButton',
	template: '<button type="button" class="comparison-state-button-stub" />',
});
const scrollStub = defineComponent({
	name: 'QScroll',
	props: { scrollId: { type: String, default: undefined } },
	setup(props, { slots, expose }) {
		const element = ref<HTMLElement | null>(null);
		onMounted(() => {
			const node = element.value;
			if (!node)
				return;

			const width = 960;
			const height = 600;
			Object.defineProperties(node, {
				clientWidth: { configurable: true, value: width },
				clientHeight: { configurable: true, value: height },
				offsetWidth: { configurable: true, value: width },
				offsetHeight: { configurable: true, value: height },
				scrollWidth: { configurable: true, value: width },
				scrollHeight: { configurable: true, value: height },
			});
			node.getBoundingClientRect = () => ({
				x: 0,
				y: 0,
				top: 0,
				right: width,
				bottom: height,
				left: 0,
				width,
				height,
				toJSON: () => ({}),
			} as DOMRect);
		});
		expose({ getScrollTarget: () => element.value });
		return () => h('div', { ref: element, id: props.scrollId, class: 'scroll-stub' }, slots.default?.());
	},
});

describe('Shared poster presentation', () => {
	let pinia = createPinia();
	beforeAll(() => baseSetup());
	beforeEach(() => {
		pinia = createPinia();
		setActivePinia(pinia);
	});

	function plugins() {
		return [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })];
	}

	test('preserves generic poster controller download and details actions through the shared card', async () => {
		const mediaItem = generatePlexMedia({
			config: { seed: 901 },
			partialData: {
				id: 31,
				type: PlexMediaType.Movie,
				plexLibraryId: 4,
				plexServerId: 2,
				title: 'Existing movie',
				hasThumb: false,
			},
		});
		const wrapper = await mountSuspended(MediaPoster, {
			props: { mediaItem },
			global: {
				plugins: plugins(),
				stubs: {
					QGlowContainer: glowContainerStub,
					QText: textStub,
					QMediaTypeIcon: mediaTypeIconStub,
					BaseButton: baseButtonStub,
					QLoadingOverlay: loadingOverlayStub,
					MediaQuality: mediaQualityStub,
					MediaComparisonStateButton: comparisonStateButtonStub,
				},
			},
		});
		expect(wrapper.findAll('.media-poster--title')).toHaveLength(2);

		const buttons = wrapper.findAllComponents({ name: 'BaseButton' });
		expect(buttons).toHaveLength(2);
		await buttons[0]!.trigger('click');
		expect(wrapper.emitted('download')?.[0]?.[0]).toEqual([
			expect.objectContaining({
				type: PlexMediaType.Movie,
				mediaIds: [31],
				plexLibraryId: 4,
				plexServerId: 2,
			}),
		]);
		await buttons[1]!.trigger('click');
		expect(wrapper.emitted('open-media-details')).toHaveLength(1);
	});
	test('keeps poster keyboard navigation and nested actions without image selection controls', async () => {
		const wrapper = await mountSuspended(MediaPosterCard, {
			props: {
				title: 'Summer trip',
				mediaType: PlexMediaType.PhotoAlbum,
				fallbackIcon: 'mdi-image-off-outline',
			},
			attrs: { tabindex: 0, role: 'group', 'aria-label': 'Summer trip' },
			slots: {
				actions: () => h('button', {
					type: 'button',
					'data-cy': 'nested-action',
					onClick: (event: MouseEvent) => event.stopPropagation(),
				}, 'View'),
			},
			global: {
				plugins: plugins(),
				stubs: {
					QGlowContainer: glowContainerStub,
					QText: textStub,
					QMediaTypeIcon: mediaTypeIconStub,
				},
			},
		});

		expect(wrapper.find('[role="checkbox"]').exists()).toBe(false);
		const card = wrapper.get('.media-poster-card');
		expect(card.attributes('tabindex')).toBe('0');
		expect(card.attributes('role')).toBe('group');
		expect(card.attributes('aria-label')).toBe('Summer trip');
		expect(wrapper.get('.media-poster--title').text()).toContain('Summer trip');
		const action = wrapper.get('[data-cy="nested-action"]');

		await card.trigger('click');
		await card.trigger('keydown', { key: 'Enter' });
		await card.trigger('keydown', { key: ' ' });
		expect(wrapper.emitted('click')).toHaveLength(3);
		expect(wrapper.emitted('click')?.[1]?.[0]).toBeInstanceOf(MouseEvent);

		await action.trigger('keydown', { key: 'Enter' });
		await action.trigger('keydown', { key: ' ' });
		await action.trigger('click');
		expect(wrapper.emitted('click')).toHaveLength(3);
	});

	test('controlled grids render through the item slot without generic media requests or scroll state writes', async () => {
		const mediaOverviewStore = useMediaOverviewStore();
		const getRange = vi.spyOn(mediaOverviewStore, 'getMediaItemsForRange');
		const requestRange = vi.spyOn(mediaOverviewStore, 'requestRange');
		const setScrollIndex = vi.spyOn(mediaOverviewStore, 'setCurrentScrollIndex');
		const item = vi.fn(({ index }: { index: number }) => h('button', {
			class: 'controlled-item',
			'data-photo-overview-index': index,
			tabindex: 0,
		}, `Photo ${index}`));

		const wrapper = await mountSuspended(PosterTable, {
			attachTo: document.body,
			props: {
				itemCount: 3,
				scrollId: 'photo-albums-1',
			},
			slots: { item },
			global: {
				plugins: plugins(),
				stubs: {
					QScroll: scrollStub,
				},
			},
		});
		try {
			await nextTick();
			await nextTick();
			expect(wrapper.find('#photo-albums-1').exists()).toBe(true);
			expect(wrapper.findAll('.controlled-item')).toHaveLength(3);
			expect(item.mock.calls.map(([payload]) => payload.index)).toEqual(expect.arrayContaining([0, 1, 2]));
			expect(getRange).not.toHaveBeenCalled();
			expect(requestRange).not.toHaveBeenCalled();
			expect(setScrollIndex).not.toHaveBeenCalled();
			const exposed = wrapper.vm as unknown as { focusItem: (index: number) => Promise<boolean> }; // defineExpose is not reflected by Vue Test Utils' component instance type.
			const focused = await exposed.focusItem(2);
			expect(focused).toBe(true);
			expect(document.activeElement?.getAttribute('data-photo-overview-index')).toBe('2');
		} finally {
			wrapper.unmount();
		}
	});
});
