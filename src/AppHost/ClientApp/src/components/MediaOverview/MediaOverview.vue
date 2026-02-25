<template>
	<!--	Refresh Library Screen	-->
	<MediaOverviewRefresh
		v-if="libraryStore.getIsLibrarySyncing(libraryId)"
		:library-id="libraryId" />
	<template v-else>
		<div class="media-overview-bar">
			<!--	Overview bar	-->
			<MediaOverviewBar
				:detail-mode="false"
				:library-id="libraryId"
				@action="onAction" />
		</div>
		<div class="media-overview-content">
			<template v-if="!mediaOverviewStore.loading">
				<!-- Media Overview -->
				<template v-if="mediaOverviewStore.itemsLength && !mediaOverviewStore.hasNoSearchResults">
					<!--	Data table display	-->
					<QRow align="start">
						<QCol>
							<template v-if="mediaOverviewStore.getMediaViewMode === ViewMode.Table">
								<MediaTable
									:disable-hover-click="mediaOverviewStore.getMediaType !== PlexMediaType.TvShow"
									:rows="mediaOverviewStore.getMediaItems"
									is-scrollable />
							</template>

							<!-- Poster display -->
							<template v-else>
								<PosterTable
									:items="mediaOverviewStore.getMediaItems"
									:library-id="libraryId"
									:media-type="mediaOverviewStore.getMediaType" />
							</template>
						</QCol>
						<!-- Alphabet Navigation -->
						<AlphabetNavigation />
					</QRow>
				</template>
				<!-- No Media Overview - Error Messages -->
				<template v-else>
					<QRow
						class="q-mt-md"
						justify="center"
						gutter="md">
						<QCol cols="auto">
							<QAlert
								type="warning">
								<template v-if="mediaOverviewStore.allMediaMode">
									{{ t('components.media-overview.no-media-items-available') }}
								</template>
								<template v-else-if="mediaOverviewStore.hasNoSearchResults">
									{{ t('components.media-overview.no-search-results', { query: mediaOverviewStore.filterQuery }) }}
								</template>
								<template v-else-if="mediaOverviewStore.hasNoFilterResults">
									{{ t('components.media-overview.no-filter-results') }}
								</template>
								<template v-else-if="libraryStore.getLibrary(libraryId)?.syncedAt === null">
									{{ t('components.media-overview.library-not-yet-synced') }}
								</template>
								<template v-else-if="!mediaOverviewStore.itemsLength">
									{{ t('components.media-overview.no-data') }}
								</template>
								<template v-else>
									{{ t('components.media-overview.could-not-display') }}
								</template>
							</QAlert>
						</QCol>
					</QRow>
				</template>
			</template>

			<!-- Media Selection Dialog -->
			<MediaSelectionDialog />
			<!-- Media Options Dialog -->
			<MediaOptionsDialog @closed="onOptionsClosed" />
			<!-- Loading overlay -->
			<QLoadingOverlay :loading="!libraryStore.getIsLibrarySyncing(libraryId) && mediaOverviewStore.loading" />
			<!-- Download confirmation dialog	-->
			<DownloadConfirmation @download="downloadStore.downloadMedia($event)" />
		</div>
	</template>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { type DownloadMediaDTO, LibrarySyncJobStatus, PlexMediaType, ViewMode } from '@dto';
import { DialogType } from '@enums';
import type { IMediaOverviewBarActions } from '@interfaces';
import {
	listenMediaOverviewDownloadCommand,
	sendMediaOverviewDownloadCommand,
	useDialogStore,
	useDownloadStore,
	useI18n,
	useLibraryStore,
	useMediaOverviewBarDownloadCommandBus,
	useMediaOverviewStore,
	useSettingsStore,
} from '#imports';

const { t } = useI18n();
const settingsStore = useSettingsStore();
const mediaOverviewStore = useMediaOverviewStore();
const downloadStore = useDownloadStore();
const libraryStore = useLibraryStore();
const dialogStore = useDialogStore();
const backgroundJobsStore = useBackgroundJobsStore();

const props = defineProps<{
	libraryId: number;
}>();

function resetProgress() {
	libraryStore.updateLibraryProgress({
		plexLibraryId: mediaOverviewStore.libraryId,
		percentage: 0,
		received: 0,
		total: 0,
		isComplete: false,
		timeStamp: '',
		timeRemaining: '',
		items: [],
		errors: [],
	});
}

function refreshLibrary() {
	resetProgress();
	useSubscription(
		libraryStore.reSyncLibrary(mediaOverviewStore.libraryId).subscribe(),
	);
}

// region Eventbus

/**
 * Listen for process download command
 */
listenMediaOverviewDownloadCommand((command) => {
	Log.info('MediaOverview => Received download command', command);
	// Only show if there is more than 1 selection
	if (command.length > 0 && command.some((x) => x.mediaIds.length > 0)) {
		if (settingsStore.isConfirmationEnabled(mediaOverviewStore.getMediaType)) {
			dialogStore.openMediaConfirmationDownloadDialog(command);
		} else {
			downloadStore.downloadMedia({
				customDestinationFolderPath: '',
				destinationFolderPathId: null,
				downloadMedias: command,
			});
		}
	}
});

useMediaOverviewBarDownloadCommandBus().on(() => {
	const downloadCommand: DownloadMediaDTO = {
		plexServerId: libraryStore.getServerByLibraryId(mediaOverviewStore.libraryId)?.id ?? 0,
		plexLibraryId: mediaOverviewStore.libraryId,
		mediaIds: mediaOverviewStore.selection.keys,
		type: mediaOverviewStore.getMediaType,
		qualities: [],
		keepCompletedInDownloadFolder: settingsStore.downloadManagerSettings.keepCompletedInDownloadFolder,
	};
	sendMediaOverviewDownloadCommand([downloadCommand]);
});

function onAction(event: IMediaOverviewBarActions) {
	switch (event) {
		case 'back':
			break;
		case 'selection-dialog':
			dialogStore.openDialog(DialogType.MediaSelectionDialog);
			break;
		case 'refresh-library':
			refreshLibrary();
			break;
		case 'media-options-dialog':
			dialogStore.openDialog(DialogType.MediaOptionsDialog);
			break;
		default:
			Log.error('Unknown action event', event);
			break;
	}
}

function onOptionsClosed(hasChanged: boolean) {
	if (hasChanged) {
		useSubscription(mediaOverviewStore.requestMedia().subscribe());
	}
}

onMounted(() => {
	resetProgress();

	// Initialize the library in the store
	useSubscription(
		mediaOverviewStore.initializeLibrary(props.libraryId).subscribe(),
	);

	// Library sync job subscription
	useSubscription(backgroundJobsStore.getLibrarySyncJobUpdate().subscribe((value) => {
		const queue = value.data;
		if (queue.plexLibraryId !== mediaOverviewStore.libraryId) {
			return;
		}

		if (queue.status === LibrarySyncJobStatus.Completed) {
			useSubscription(mediaOverviewStore.requestMedia().subscribe());
		}
	}));
});
</script>

<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

#media-container,
.media-table-container,
.detail-view-container {
  width: 100%;
  overflow: hidden;
}

.progress-table {
  margin: 0 auto;
  border-collapse: collapse;

  td {
    vertical-align: middle;
    white-space: nowrap;
  }

  .progress-bar-cell {
    width: 250px;
  }
}
</style>
