import { beforeAll, describe, expect, test } from 'vitest';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { baseSetup } from '@services-test-base';
import AdvancedSettings from '@/pages/settings/advanced.vue';

const stub = (dataCy: string) => ({
	template: `<div data-cy="${dataCy}" />`,
});

describe('Advanced settings page', () => {
	beforeAll(() => baseSetup());

	test('Should render debug settings after every other advanced settings section', async () => {
		// Arrange
		const wrapper = await mountSuspended(AdvancedSettings, {
			global: {
				stubs: {
					QPage: { template: '<main class="settings-page"><slot /></main>' },
					DownloadManagerSection: stub('download-manager-section'),
					DownloadScheduleSection: stub('download-schedule-section'),
					NetworkSection: stub('network-section'),
					SetupSection: stub('setup-section'),
					DebugSection: stub('settings-debug-section'),
				},
			},
		});

		// Act
		const sections = Array.from(wrapper.find('.settings-page').element.children);

		// Assert
		expect(sections.at(-1)?.getAttribute('data-cy')).toBe('settings-debug-section');
		wrapper.unmount();
	});
});
