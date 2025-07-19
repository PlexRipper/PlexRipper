<template>
	<!--	Refresh Library Screen	-->
	<QRow
		v-if="isRefreshing"
		align="start"
		class="q-pt-xl"
		cy="refresh-library-container"
		full-height>
		<QCol
			text-align="center">
			<ProgressComponent
				:percentage="libraryProgress?.percentage ?? -1"
				:text="refreshingText"
				circular-mode
				:indeterminate="libraryProgress?.percentage == 0"
				class="q-my-lg" />
			<template v-if="libraryProgress?.percentage != 0">
				<QText
					:value="$t('components.media-overview.steps-remaining', {
						index: libraryProgress?.step,
						total: libraryProgress?.totalSteps,
					})"
					align="center" />
				<QCountdown
					:value="libraryProgress?.timeRemaining ?? ''" />
			</template>
		</QCol>
	</QRow>
	<template v-else>
		<div class="media-overview-bar">
			<!--	Overview bar	-->
			<MediaOverviewBar
				:detail-mode="false"
				:all-media-mode="allMediaMode"
				:library-id="libraryId"
				:media-type="mediaOverviewStore.mediaType"
				@action="onAction" />
		</div>
		<div class="media-overview-content">
			<!-- Media Overview -->
			<template v-if="!mediaOverviewStore.loading && mediaOverviewStore.itemsLength">
				<template v-if="mediaOverviewStore.hasNoSearchResults">
					<QAlert type="warning">
						<QText
							:value="t('components.media-overview.no-search-results', { query: mediaOverviewStore.filterQuery })" />
					</QAlert>
				</template>
				<template v-else>
					<!--	Data table display	-->
					<QRow align="start">
						<QCol>
							<template v-if="mediaOverviewStore.getMediaViewMode === ViewMode.Table">
								<MediaTable
									:disable-hover-click="mediaType !== PlexMediaType.TvShow"
									:rows="mediaOverviewStore.getMediaItems"
									is-scrollable />
							</template>

							<!-- Poster display -->
							<template v-else>
								<PosterTable
									:items="mediaOverviewStore.getMediaItems"
									:library-id="libraryId"
									:media-type="mediaType" />
							</template>
						</QCol>
						<!-- Alphabet Navigation -->
						<AlphabetNavigation />
					</QRow>
				</template>
			</template>

			<!-- No Media Overview -->
			<template v-else-if="!mediaOverviewStore.loading">
				<QRow
					class="q-mt-md"
					justify="center"
					gutter="md">
					<QCol cols="auto">
						<QAlert
							type="warning">
							<template v-if="mediaOverviewStore.allMediaMode">
								{{ $t('components.media-overview.no-media-items-available') }}
							</template>
							<template v-else-if="library?.syncedAt === null">
								{{ $t('components.media-overview.library-not-yet-synced') }}
							</template>
							<template v-else-if="!mediaOverviewStore.itemsLength">
								{{ $t('components.media-overview.no-data') }}
							</template>
							<template v-else>
								{{ $t('components.media-overview.could-not-display') }}
							</template>
						</QAlert>
					</QCol>
				</QRow>
			</template>
			<!-- Media Selection Dialog -->
			<MediaSelectionDialog />
			<!-- Media Options Dialog -->
			<MediaOptionsDialog @closed="onOptionsClosed" />
			<!-- Loading overlay -->
			<QLoadingOverlay :loading="!isRefreshing && mediaOverviewStore.loading" />
			<!-- Download confirmation dialog	-->
			<DownloadConfirmation @download="downloadStore.downloadMedia($event)" />
		</div>
	</template>
</template>

<script setup lang="ts">
import Log from 'consola';
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { type DownloadMediaDTO, type LibraryProgress, PlexMediaType, ViewMode } from '@dto';
import { DialogType } from '@enums';
import type { IMediaOverviewBarActions } from '@interfaces';
import {
	useMediaOverviewBarDownloadCommandBus,
	useMediaOverviewSortBus,
	listenMediaOverviewDownloadCommand,
	sendMediaOverviewDownloadCommand,
	useSignalrStore,
	useMediaOverviewStore,
	useSettingsStore,
	useDownloadStore,
	useLibraryStore,
	useServerStore,
	useDialogStore,
	useI18n,
} from '#imports';

const { t } = useI18n();
const settingsStore = useSettingsStore();
const mediaOverviewStore = useMediaOverviewStore();
const downloadStore = useDownloadStore();
const libraryStore = useLibraryStore();
const serverStore = useServerStore();
const dialogStore = useDialogStore();
const signalRStore = useSignalrStore();

const isRefreshing = ref(false);

const libraryProgress = ref<LibraryProgress | null>(null);

const props = withDefaults(defineProps<{
	libraryId: number;
	mediaType: PlexMediaType;
	allMediaMode?: boolean;
}>(), {
	allMediaMode: false,
});

const library = computed(() => libraryStore.getLibrary(mediaOverviewStore.libraryId));

const refreshingText = computed(() => {
	const server = libraryStore.getServerByLibraryId(mediaOverviewStore.libraryId);
	return t('components.media-overview.is-refreshing', {
		library: get(library) ? libraryStore.getLibraryName(mediaOverviewStore.libraryId) : t('general.commands.unknown'),
		server: server ? serverStore.getServerName(server.id) : t('general.commands.unknown'),
	});
});

function resetProgress(isRefreshingValue: boolean) {
	set(isRefreshing, isRefreshingValue);

	set(libraryProgress, {
		id: mediaOverviewStore.libraryId,
		percentage: 0,
		received: 0,
		total: 0,
		isRefreshing: isRefreshingValue,
		isComplete: false,
		timeStamp: '',
		timeRemaining: '',
		step: 0,
		totalSteps: 0,
	});
}

function refreshLibrary() {
	set(isRefreshing, true);
	resetProgress(true);
	useSubscription(
		libraryStore.reSyncLibrary(mediaOverviewStore.libraryId).subscribe({
			complete: () => {
				set(isRefreshing, false);
			},
		}),
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
		if (settingsStore.isConfirmationEnabled(props.mediaType)) {
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
		type: props.mediaType,
	};
	sendMediaOverviewDownloadCommand([downloadCommand]);
});

useMediaOverviewSortBus().on((event) => {
	mediaOverviewStore.sortMedia(event);
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
		useSubscription(
			mediaOverviewStore.requestMedia().subscribe());
	}
}

onMounted(() => {
	resetProgress(false);
	set(isRefreshing, false);

	mediaOverviewStore.$patch({
		libraryId: props.libraryId,
		mediaType: props.mediaType,
		isDetailView: false,
	});

	// Initial data load
	useSubscription(mediaOverviewStore.requestMedia().subscribe());

	if (!props.allMediaMode) {
		useSubscription(
			signalRStore.getLibraryProgress(mediaOverviewStore.libraryId)
				.subscribe((data) => {
					if (data) {
						set(libraryProgress, data);
						set(isRefreshing, data.isRefreshing);
						if (data.isComplete) {
							set(isRefreshing, false);
							useSubscription(mediaOverviewStore.requestMedia().subscribe());
						}
					}
				}),
		);
	}
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
</style>
