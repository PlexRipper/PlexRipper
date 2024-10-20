<template>
	<!--	Refresh Library Screen	-->
	<QRow
		v-if="isRefreshing"
		full-height
		align="start"
		class="q-pt-xl"
		cy="refresh-library-container">
		<QCol
			text-align="center">
			<ProgressComponent
				circular-mode
				class="q-my-lg"
				:percentage="libraryProgress?.percentage ?? -1"
				:text="refreshingText" />
			<QText
				align="center"
				:value="$t('components.media-overview.steps-remaining', {
					index: libraryProgress?.step,
					total: libraryProgress?.totalSteps,
				})" />
			<QCountdown
				:value="libraryProgress?.timeRemaining ?? ''" />
		</QCol>
	</QRow>
	<template v-else>
		<!--	Overview bar	-->
		<MediaOverviewBar
			:server="
				libraryStore.getServerByLibraryId(libraryId)"
			:library="library"
			:detail-mode="false"
			@view-change="changeView"
			@selection-dialog="useOpenControlDialog(mediaSelectionDialogName)"
			@refresh-library="refreshLibrary" />

		<!-- Media Overview -->
		<template v-if="!loading && mediaOverviewStore.itemsLength">
			<template v-if="mediaOverviewStore.hasNoSearchResults">
				<QAlert type="warning">
					<QText :value="t('components.media-overview.no-search-results', { query: mediaOverviewStore.filterQuery })" />
				</QAlert>
			</template>
			<template v-else>
				<!--	Data table display	-->
				<QRow
					id="media-container"
					align="start">
					<QCol>
						<template v-if="mediaOverviewStore.getMediaViewMode === ViewMode.Table">
							<MediaTable
								:rows="mediaOverviewStore.getMediaItems"
								:disable-hover-click="mediaType !== PlexMediaType.TvShow"
								is-scrollable />
						</template>

						<!-- Poster display -->
						<template v-else>
							<PosterTable
								:library-id="libraryId"
								:media-type="mediaType"
								:items="mediaOverviewStore.getMediaItems" />
						</template>
					</QCol>
					<!-- Alphabet Navigation -->
					<AlphabetNavigation />
				</QRow>
			</template>
		</template>

		<!-- No Media Overview -->
		<template v-else-if="!loading">
			<QRow justify="center">
				<QCol cols="auto">
					<QAlert type="warning">
						<template v-if="library?.syncedAt === null">
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
		<MediaSelectionDialog :name="mediaSelectionDialogName" />
		<!--	Loading overlay	-->
		<QLoadingOverlay :loading="!isRefreshing && loading" />
		<!--		Download confirmation dialog	-->
		<DownloadConfirmation
			:name="downloadConfirmationName"
			@download="downloadStore.downloadMedia($event)" />
	</template>
</template>

<script setup lang="ts">
import Log from 'consola';
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { type DownloadMediaDTO, type LibraryProgress, PlexMediaType, ViewMode } from '@dto';
import {
	useMediaOverviewBarDownloadCommandBus,
	useMediaOverviewSortBus,
	useOpenControlDialog,
	listenMediaOverviewDownloadCommand,
	sendMediaOverviewDownloadCommand,
	useMediaStore,
	useMediaOverviewStore,
	useSettingsStore,
	useDownloadStore,
	useLibraryStore,
	useServerStore,
	useI18n,
} from '#imports';

// region SetupFields
const { t } = useI18n();
const settingsStore = useSettingsStore();
const mediaOverviewStore = useMediaOverviewStore();
const downloadStore = useDownloadStore();
const libraryStore = useLibraryStore();
const serverStore = useServerStore();

// endregion

const downloadConfirmationName = 'mediaDownloadConfirmation';
const mediaSelectionDialogName = 'mediaSelectionDialogName';
const isRefreshing = ref(false);

const libraryProgress = ref<LibraryProgress | null>(null);

const loading = ref(false);

const props = defineProps<{
	libraryId: number;
	mediaType: PlexMediaType;
}>();

const library = computed(() => libraryStore.getLibrary(props.libraryId));

const isConfirmationEnabled = computed(() => {
	switch (props.mediaType) {
		case PlexMediaType.Movie:
			return settingsStore.confirmationSettings.askDownloadMovieConfirmation;
		case PlexMediaType.TvShow:
			return settingsStore.confirmationSettings.askDownloadTvShowConfirmation;
		case PlexMediaType.Season:
			return settingsStore.confirmationSettings.askDownloadSeasonConfirmation;
		case PlexMediaType.Episode:
			return settingsStore.confirmationSettings.askDownloadEpisodeConfirmation;
		default:
			return true;
	}
});

const refreshingText = computed(() => {
	const server = libraryStore.getServerByLibraryId(props.libraryId);
	return t('components.media-overview.is-refreshing', {
		library: get(library) ? libraryStore.getLibraryName(props.libraryId) : t('general.commands.unknown'),
		server: server ? serverStore.getServerName(server.id) : t('general.commands.unknown'),
	});
});

function changeView(viewMode: ViewMode) {
	mediaOverviewStore.clearSort();
	settingsStore.updateDisplayMode(props.mediaType, viewMode);
}

function resetProgress(isRefreshingValue: boolean) {
	set(isRefreshing, isRefreshingValue);

	set(libraryProgress, {
		id: props.libraryId,
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
		libraryStore.reSyncLibrary(props.libraryId).subscribe({
			complete: () => {
				set(isRefreshing, false);
			},
		}),
	);
}

function onRequestMedia({ page = 0, size = 0 }: { page: number; size: number }) {
	if (get(loading)) {
		return;
	}
	set(loading, true);

	useSubscription(
		useMediaStore()
			.getMediaData(props.libraryId, page, size)
			.subscribe({
				next: (mediaData) => {
					if (!mediaData) {
						Log.error(`MediaOverview => No media data for library id ${props.libraryId} was found`);
					}
					mediaOverviewStore.setMedia(mediaData, props.mediaType);
				},
				error: (error) => {
					Log.error(`MediaOverview => Error while server and mediaData for library id ${props.libraryId}:`, error);
				},
				complete: () => {
					set(loading, false);
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
		if (isConfirmationEnabled.value) {
			useOpenControlDialog(downloadConfirmationName, command);
		} else {
			downloadStore.downloadMedia(command);
		}
	}
});

useMediaOverviewBarDownloadCommandBus().on(() => {
	const downloadCommand: DownloadMediaDTO = {
		plexServerId: libraryStore.getServerByLibraryId(props.libraryId)?.id ?? 0,
		plexLibraryId: props.libraryId,
		mediaIds: mediaOverviewStore.selection.keys,
		type: props.mediaType,
	};
	sendMediaOverviewDownloadCommand([downloadCommand]);
});

useMediaOverviewSortBus().on((event) => {
	mediaOverviewStore.sortMedia(event);
});

onMounted(() => {
	resetProgress(false);
	set(isRefreshing, false);

	if (!props.libraryId) {
		Log.error('Library id was not provided');
		return;
	}

	// Initial data load
	onRequestMedia({
		page: 0,
		size: 0,
	});

	useSubscription(
		useSignalrStore()
			.getLibraryProgress(props.libraryId)
			.subscribe((data) => {
				if (data) {
					set(libraryProgress, data);
					set(isRefreshing, data.isRefreshing);
					if (data.isComplete) {
						onRequestMedia({ size: 0, page: 0 });
						set(isRefreshing, false);
					}
				}
			}),
	);
});
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

#media-container,
.media-table-container,
.detail-view-container {
	// We need a set height so we calculate the remaining content space by subtracting other component heights
	height: calc(100vh - $app-bar-height - $media-overview-bar-height);
	width: 100%;
	overflow: hidden;
}
</style>
