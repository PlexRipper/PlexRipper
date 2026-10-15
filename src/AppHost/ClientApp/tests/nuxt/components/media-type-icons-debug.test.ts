import { beforeAll, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { baseSetup } from '@services-test-base';
import { PlexMediaType } from '@dto';
import Convert from '@class/Convert';
import MediaTypeIconsDebug from '@components/DebugTools/MediaTypeIconsDebug.vue';

describe('MediaTypeIconsDebug', () => {
	beforeAll(() => baseSetup());

	test('Should render every Plex media type with its resolved icon', async () => {
		// Arrange
		const pinia = createPinia();
		setActivePinia(pinia);
		const mediaTypes = Object.values(PlexMediaType);

		// Act
		const wrapper = await mountSuspended(MediaTypeIconsDebug, {
			global: {
				plugins: [pinia],
			},
		});

		// Assert
		expect(wrapper.findAll('.media-type-icons-debug__card')).toHaveLength(mediaTypes.length);
		for (const mediaType of mediaTypes) {
			const card = wrapper.get(`[data-cy="media-type-icon-${mediaType}"]`);
			expect(card.text()).toContain(mediaType);
			expect(card.text()).toContain(Convert.mediaTypeToIcon(mediaType));
			expect(card.find('.q-media-type-icon').classes()).toContain(Convert.mediaTypeToIcon(mediaType));
		}
		wrapper.unmount();
	});
});
