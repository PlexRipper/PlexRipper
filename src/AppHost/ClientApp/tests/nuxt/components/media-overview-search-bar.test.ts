import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import { baseSetup } from '@services-test-base';
import { useMediaOverviewStore, usePhotoBrowsingStore } from '@store';
import messages from '@/lang/en-US.json';
import MediaOverviewSearchBar from '@/components/MediaOverview/MediaOverviewSearchBar.vue';

const i18n = () => createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } });

describe('MediaOverviewSearchBar - photo mode', () => {
	let pinia = createPinia();

	beforeAll(() => baseSetup());
	beforeEach(() => {
		pinia = createPinia();
		setActivePinia(pinia);
	});

	test('forwards global photo search and does not render generic filters', async () => {
		// Arrange
		const photoStore = usePhotoBrowsingStore();
		const mediaOverviewStore = useMediaOverviewStore();
		photoStore.searchQuery = 'existing';
		mediaOverviewStore.filterQuery = 'generic';
		const wrapper = await mountSuspended(MediaOverviewSearchBar, {
			props: { photoMode: true, libraryId: 4 },
			global: { plugins: [pinia, i18n()] },
		});

		// Act
		const input = wrapper.findComponent({ name: 'QInput' });
		await input.vm.$emit('update:modelValue', 'beach');

		// Assert
		expect(wrapper.emitted('photo-search')).toEqual([['beach']]);
		expect(wrapper.get('[data-cy="photo-search-input"]').attributes('aria-label')).toBe('Search photos');
		expect(wrapper.text()).not.toContain('Searches all enabled Photos libraries');
		expect(wrapper.find('[data-cy="media-overview-filter-btn"]').exists()).toBe(false);
		expect(mediaOverviewStore.filterQuery).toBe('generic');
		wrapper.unmount();
	});
});
