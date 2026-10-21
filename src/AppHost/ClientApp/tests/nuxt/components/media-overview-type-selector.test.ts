import { beforeAll, describe, expect, test } from 'vitest';
import { createI18n } from 'vue-i18n';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { baseSetup } from '@services-test-base';
import { PlexMediaType } from '@dto';
import messages from '@/lang/en-US.json';
import MediaOverviewTypeSelector from '@/components/MediaOverview/MediaOverviewTypeSelector.vue';

describe('MediaOverviewTypeSelector', () => {
	beforeAll(() => baseSetup());

	test('offers every other media family, including Photos, from the current family', async () => {
		const wrapper = await mountSuspended(MediaOverviewTypeSelector, {
			props: { selectedType: PlexMediaType.Movie },
			global: { plugins: [createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })] },
		});

		expect(wrapper.find(`[data-cy="media-overview-type-${PlexMediaType.Movie}"]`).exists()).toBe(false);
		expect(wrapper.find(`[data-cy="media-overview-type-${PlexMediaType.PhotoAlbum}"]`).exists()).toBe(true);
		await wrapper.get(`[data-cy="media-overview-type-${PlexMediaType.PhotoAlbum}"]`).trigger('click');
		expect(wrapper.emitted('select')?.[0]).toEqual([PlexMediaType.PhotoAlbum]);
	});
});
