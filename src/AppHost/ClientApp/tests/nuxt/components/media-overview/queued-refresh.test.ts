import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createPinia, setActivePinia, type Pinia } from 'pinia';
import { defineComponent } from 'vue';
import { createI18n } from 'vue-i18n';
import { baseSetup } from '@services-test-base';
import { generateLibrarySyncJobQueue, generatePlexLibrary, Seed } from '@mock';
import { LibrarySyncJobStatus, PlexMediaType } from '@dto';
import { useDialogStore, useLibraryStore, useServerStore } from '@store';
import MediaOverviewRefresh from '@/components/MediaOverview/MediaOverviewRefresh.vue';
import messages from '@/lang/en-US.json';

const passthrough = defineComponent({
	inheritAttrs: false,
	template: '<div v-bind="$attrs"><slot name="avatar" /><slot /></div>',
});

const buttonStub = defineComponent({
	inheritAttrs: false,
	props: {
		cy: { type: String, default: '' },
		label: { type: String, default: '' },
	},
	template: '<button v-bind="$attrs" :data-cy="cy"><slot />{{ label }}</button>',
});

describe('MediaOverviewRefresh queued presentation', () => {
	let pinia: Pinia;
	beforeAll(() => baseSetup());
	beforeEach(() => {
		pinia = createPinia();
		setActivePinia(pinia);
	});
	afterEach(() => {
		vi.restoreAllMocks();
	});

	function setupQueuedLibrary(syncedAt: string | null) {
		const libraryStore = useLibraryStore();
		const serverStore = useServerStore();
		const processingLibrary = generatePlexLibrary({
			seed: new Seed(1801),
			plexServerId: 7,
			type: PlexMediaType.Movie,
			partialData: { id: 701, title: 'Movies', syncedAt: new Date().toISOString() },
		});
		const queuedLibrary = generatePlexLibrary({
			seed: new Seed(1802),
			plexServerId: 7,
			type: PlexMediaType.Movie,
			partialData: { id: 702, title: 'Queued Movies', syncedAt },
		});
		libraryStore.updateLibrary(processingLibrary);
		libraryStore.updateLibrary(queuedLibrary);
		const processingQueue = generateLibrarySyncJobQueue({
			plexLibraryId: processingLibrary.id,
			plexServerId: 7,
			status: LibrarySyncJobStatus.Processing,
		});
		const queuedQueue = generateLibrarySyncJobQueue({
			plexLibraryId: queuedLibrary.id,
			plexServerId: 7,
			status: LibrarySyncJobStatus.Queued,
		});
		libraryStore.updateSyncQueue(processingQueue);
		libraryStore.updateSyncQueue(queuedQueue);
		vi.spyOn(libraryStore, 'getLibrarySyncWaitingReason').mockReturnValue('busy');
		vi.spyOn(libraryStore, 'getProcessingLibrarySyncQueue').mockReturnValue(processingQueue);
		vi.spyOn(serverStore, 'getServerName').mockReturnValue('My Plex Server');
		return { libraryStore, queuedLibrary };
	}

	async function render(libraryId: number, variant: 'panel' | 'banner') {
		return mountSuspended(MediaOverviewRefresh, {
			props: { libraryId, mode: 'queued', variant },
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: {
					QBanner: passthrough,
					QCol: passthrough,
					QIcon: passthrough,
					QRow: passthrough,
					BaseButton: buttonStub,
				},
			},
		});
	}

	test('renders a first-sync queued panel without progress or execution controls', async () => {
		const { queuedLibrary } = setupQueuedLibrary(null);
		const wrapper = await render(queuedLibrary.id, 'panel');

		expect(wrapper.find('[data-cy="media-overview-library-sync-queued-panel"]').exists()).toBe(true);
		expect(wrapper.find('[data-cy="media-overview-library-sync-queued-message"]').text()).toContain(
			'Waiting for Movies to finish refreshing on My Plex Server.',
		);
		expect(wrapper.find('[data-cy="media-overview-library-sync-queued-panel"]').attributes('role')).toBe('status');
		expect(wrapper.find('[data-cy*="progress"]').exists()).toBe(false);
		expect(wrapper.find('[data-cy*="eta"]').exists()).toBe(false);
	});

	test('renders a previously synced queued banner with activity and cancellation actions', async () => {
		const { queuedLibrary } = setupQueuedLibrary(new Date().toISOString());
		const dialogStore = useDialogStore();
		const openActivity = vi.spyOn(dialogStore, 'openSyncServerMediaDialog');
		const wrapper = await render(queuedLibrary.id, 'banner');

		expect(wrapper.find('[data-cy="media-overview-library-sync-queued-banner"]').exists()).toBe(true);
		expect(wrapper.find('[data-cy="view-library-sync-activity-button"]').exists()).toBe(true);
		expect(wrapper.find('[data-cy="cancel-queued-library-sync-button"]').exists()).toBe(true);
		expect(wrapper.find('[data-cy="media-overview-library-sync-queued-banner"]').attributes('aria-live')).toBe('polite');

		await wrapper.get('[data-cy="view-library-sync-activity-button"]').trigger('click');
		expect(openActivity).toHaveBeenCalledWith(7);
	});
});
