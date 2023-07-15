<template>
	<!--	Refresh Library Screen	-->
	<q-row v-if="isRefreshing" justify="center" align="center" class="refresh-library-container" cy="refresh-library-container">
		<q-col cols="8" align-self="center">
			<progress-component circular-mode :percentage="libraryProgress?.percentage ?? -1" :text="refreshingText" />
			<!--				<h1 v-else>{{ t('components.media-overview.retrieving-library') }}</h1>-->
		</q-col>
	</q-row>
	<!-- Media Overview -->
	<template v-else-if="!loading && mediaOverviewStore.itemsLength">
		<q-row no-gutters>
			<q-col>
				<!--	Overview bar	-->
				<media-overview-bar
					:server="libraryStore.getServerByLibraryId(props.libraryId)"
					:library="libraryStore.getLibrary(props.libraryId)"
					:detail-mode="!mediaOverviewStore.showMediaOverview"
					@back="closeDetailsOverview"
					@view-change="changeView"
					@selection-dialog="useOpenControlDialog(mediaSelectionDialogName)"
					@refresh-library="refreshLibrary" />
				<!--	Data table display	-->
				<q-row id="media-container" align="start">
					<q-col v-show="mediaOverviewStore.showMediaOverview">
						<template v-if="mediaOverviewStore.getMediaViewMode === ViewMode.Table">
							<MediaTable
								:rows="mediaOverviewStore.items"
								:disable-hover-click="mediaType !== PlexMediaType.TvShow"
								is-scrollable />
						</template>

						<!-- Poster display-->
						<template v-else>
							<poster-table :library-id="libraryId" :media-type="mediaType" :items="mediaOverviewStore.items" />
						</template>
					</q-col>

					<!-- Alphabet Navigation-->
					<alphabet-navigation v-show="mediaOverviewStore.showMediaOverview" />
				</q-row>
			</q-col>
		</q-row>
	</template>
	<!-- No Media Overview -->
	<template v-else-if="!loading && mediaOverviewStore.itemsLength === 0">
		<q-row justify="center">
			<q-col cols="auto">
				<h1>{{ t('components.media-overview.no-data') }}</h1>
			</q-col>
		</q-row>
	</template>
	<!-- Media Details Display-->
	<DetailsOverview :name="mediaDetailsDialogName" />
	<!-- Media Selection Dialog-->
	<MediaSelectionDialog :name="mediaSelectionDialogName" />
	<!--	Loading overlay	-->
	<QLoadingOverlay :loading="loading" />
	<!--		Download confirmation dialog	-->
	<DownloadConfirmation
		:name="downloadConfirmationName"
		:items="mediaOverviewStore.items"
		@download="downloadStore.downloadMedia($event)" />
</template>

<script setup lang="ts">
import Log from 'consola';
import { set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { useRouter, RouteLocationNormalized, RouteLocationNormalizedLoaded } from 'vue-router';
import type { DownloadMediaDTO } from '@dto/mainApi';
import { LibraryProgress, PlexMediaType, ViewMode } from '@dto/mainApi';
import { MediaService, SignalrService } from '@service';
import {
	useMediaOverviewBarDownloadCommandBus,
	useMediaOverviewSortBus,
	useOpenControlDialog,
	listenMediaOverviewDownloadCommand,
	useCloseControlDialog,
	sendMediaOverviewDownloadCommand,
} from '#imports';
import { listenMediaOverviewOpenDetailsCommand, sendMediaOverviewOpenDetailsCommand } from '@composables/event-bus';
import { useMediaOverviewStore, useSettingsStore, useDownloadStore, useLibraryStore, useServerStore } from '~/store';

// region SetupFields
const { t } = useI18n();
const settingsStore = useSettingsStore();
const mediaOverviewStore = useMediaOverviewStore();
const downloadStore = useDownloadStore();
const libraryStore = useLibraryStore();
const serverStore = useServerStore();
const router = useRouter();

// endregion

const downloadConfirmationName = 'mediaDownloadConfirmation';
const mediaDetailsDialogName = 'mediaDetailsDialogName';
const mediaSelectionDialogName = 'mediaSelectionDialogName';
const isRefreshing = ref(false);

const libraryProgress = ref<LibraryProgress | null>(null);

const loading = ref(true);

const props = defineProps<{
	libraryId: number;
	mediaId: number;
	mediaType: PlexMediaType;
}>();

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
	const library = libraryStore.getLibrary(props.libraryId);
	const server = libraryStore.getServerByLibraryId(props.libraryId);
	return t('components.media-overview.is-refreshing', {
		library: library ? libraryStore.getLibraryName(library.id) : t('general.commands.unknown'),
		server: server ? serverStore.getServerName(server.id) : t('general.commands.unknown'),
	});
});

function changeView(viewMode: ViewMode) {
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
	});
}

function refreshLibrary() {
	set(isRefreshing, true);
	resetProgress(true);
	useSubscription(
		libraryStore.reSyncLibrary(props.libraryId).subscribe(() => {
			set(isRefreshing, false);
		}),
	);
}

function onRequestMedia({ page = 0, size = 0 }: { page: number; size: number }) {
	useSubscription(
		MediaService.getMediaData(props.libraryId, page, size).subscribe({
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

listenMediaOverviewOpenDetailsCommand((mediaId: number) => {
	if (!mediaId) {
		Log.error('mediaId was invalid, could not open details', mediaId);
		return;
	}

	// Replace the url with the library id and media id
	router
		.push({
			name: 'details-overview',
			params: { libraryId: props.libraryId, tvShowId: mediaId },
		})
		.then(() => {
			useOpenControlDialog(mediaDetailsDialogName, { mediaId, type: props.mediaType });
			mediaOverviewStore.showMediaOverview = false;
		});
});

function closeDetailsOverview() {
	Log.info('closeDetailsOverview');
	// Replace the url with the library id
	router
		.push({
			name: 'media-overview',
			params: { libraryId: props.libraryId },
		})
		.then(() => {
			useCloseControlDialog(mediaDetailsDialogName);
			mediaOverviewStore.showMediaOverview = true;
		});
}

useMediaOverviewBarDownloadCommandBus().on(() => {
	if (mediaOverviewStore.showMediaOverview) {
		const downloadCommand: DownloadMediaDTO = {
			plexServerId: libraryStore.getServerByLibraryId(props.libraryId)?.id ?? 0,
			plexLibraryId: props.libraryId,
			mediaIds: mediaOverviewStore.selection.keys,
			type: props.mediaType,
		};
		sendMediaOverviewDownloadCommand([downloadCommand]);
	}
});

useMediaOverviewSortBus().on((event) => {
	mediaOverviewStore.sortMedia(event);
});

function setupRouter() {
	router.beforeEach((to, from, next) => {
		// From MediaOverview => DetailsOverview
		if (!from.path.includes('details') && to.path.includes('details')) {
			let tableRef: HTMLElement | null = null;
			if (mediaOverviewStore.getMediaViewMode === ViewMode.Table) {
				tableRef = document.getElementById('media-table-scroll');
			}
			if (mediaOverviewStore.getMediaViewMode === ViewMode.Poster) {
				tableRef = document.getElementById('poster-table');
			}

			if (!tableRef) {
				Log.error('tableRef was null for type', mediaOverviewStore.getMediaViewMode);
				return next();
			}
			// Save the current scroll position to be restored when navigating back
			to.meta?.scrollPos && (to.meta.scrollPos.top = tableRef.scrollTop);
		}

		return next();
	});

	router.options.scrollBehavior = (to: RouteLocationNormalized, from: RouteLocationNormalizedLoaded) => {
		// From DetailsOverview => MediaOverview
		if (from.path.includes('details') && !to.path.includes('details')) {
			return new Promise((resolve) => {
				setTimeout(() => {
					let tableRef: HTMLElement | null = null;
					switch (mediaOverviewStore.getMediaViewMode) {
						case ViewMode.Table:
							tableRef = document.getElementById('media-table-scroll');
							break;
						case ViewMode.Poster:
							tableRef = document.getElementById('poster-table');
							break;
						default:
							Log.error('Unknown mediaViewMode', mediaOverviewStore.getMediaViewMode);
							return;
					}

					if (!tableRef) {
						Log.error('tableRef was null for type', mediaOverviewStore.getMediaViewMode);
						return;
					}

					tableRef.scrollTo({
						behavior: 'smooth',
						top: from.meta.scrollPos?.top ?? -1,
						left: 0,
					});

					return resolve({
						behavior: 'smooth',
						top: from.meta.scrollPos?.top ?? -1,
						left: 0,
					});
				}, 100);
			});
		}
		return Promise.resolve();
	};
}

// endregion

onMounted(() => {
	Log.info('MediaOverview => onMounted');
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

	setupRouter();

	useSubscription(
		SignalrService.getLibraryProgress(props.libraryId).subscribe((data) => {
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

	if (props.mediaId) {
		nextTick(() => {
			sendMediaOverviewOpenDetailsCommand(props.mediaId);
		});
	}
});
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

.refresh-library-container {
	height: calc(100vh - $app-bar-height);
}

#media-container,
.media-table-container,
.detail-view-container {
	// We need a set height so we calculate the remaining content space by subtracting other component heights
	height: calc(100vh - $app-bar-height - $media-overview-bar-height);
	width: 100%;
	overflow: hidden;
}
</style>
