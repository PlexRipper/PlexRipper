<template>
	<!--	Loading screen	-->
	<!--	<template v-if="loading">-->
	<!--		<q-row justify="center" class="mx-0">-->
	<!--			<q-col cols="auto" align-self="center">-->
	<!--				<q-circular-progress size="70px" indeterminate />-->
	<!--				<h1 v-if="isRefreshing">-->
	<!--					{{-->
	<!--						$t('components.media-overview.is-refreshing', {-->
	<!--							library: library.value ? library.value.title : $t('general.commands.unknown'),-->
	<!--							server: server.value ? server.value.name : $t('general.commands.unknown'),-->
	<!--						})-->
	<!--					}}-->
	<!--				</h1>-->
	<!--				<h1 v-else>{{ $t('components.media-overview.retrieving-library') }}</h1>-->
	<!--				&lt;!&ndash; Library progress bar &ndash;&gt;-->
	<!--				<q-linear-progress :value="getPercentage" height="20" stripe color="deep-orange">-->
	<!--					<div class="absolute-full flex flex-center">-->
	<!--						<q-badge color="white" text-color="accent" :label="`${getPercentage}%`" />-->
	<!--					</div>-->
	<!--				</q-linear-progress>-->
	<!--			</q-col>-->
	<!--		</q-row>-->
	<!--	</template>-->
	<!-- Header -->
	<template v-if="!loading && items.length">
		<q-row no-gutters>
			<q-col>
				<!--	Overview bar	-->
				<media-overview-bar
					:server="server"
					:library="library"
					:view-mode="mediaViewMode"
					:has-selected="selected.keys.length > 0"
					:detail-mode="!showMediaOverview"
					:media-item="mediaItem"
					:hide-download-button="!mediaViewMode === ViewMode.Table"
					@back="closeDetailsOverview"
					@view-change="changeView"
					@refresh-library="refreshLibrary" />
				<!--	Data table display	-->
				<q-row id="media-container" align="start">
					<q-col v-show="showMediaOverview">
						<template v-if="mediaViewMode === ViewMode.Table">
							<MediaTable
								:rows="items"
								:selection="selected"
								:disable-hover-click="mediaType !== PlexMediaType.TvShow"
								:scroll-dict="scrollDict"
								is-scrollable
								@selection="selected = $event" />
						</template>

						<!-- Poster display-->
						<template v-else>
							<poster-table
								:library-id="libraryId"
								:media-type="mediaType"
								:items="items"
								:scroll-dict="scrollDict" />
						</template>
					</q-col>
					<!-- Alphabet Navigation-->
					<alphabet-navigation v-show="showMediaOverview" :scroll-dict="scrollDict" />
				</q-row>
			</q-col>
		</q-row>
	</template>
	<template v-else-if="!loading && items.length === 0">
		<q-row justify="center">
			<q-col cols="auto">
				<h1>{{ $t('components.media-overview.no-data') }}</h1>
			</q-col>
		</q-row>
	</template>
	<!-- Media Details Display-->
	<DetailsOverview :name="mediaDetailsDialogName" />
	<!--	Loading overlay	-->
	<QLoadingOverlay :loading="loading" />
	<!--		Download confirmation dialog	-->
	<DownloadConfirmation :name="downloadConfirmationName" :items="items" @download="DownloadService.downloadMedia($event)" />
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, defineProps, computed, onMounted, nextTick } from 'vue';
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { useRouter } from 'vue-router';
import { take } from 'rxjs/operators';
import { isEqual, orderBy } from 'lodash-es';
import type { DisplaySettingsDTO, DownloadMediaDTO, PlexMediaDTO, PlexMediaSlimDTO, PlexServerDTO } from '@dto/mainApi';
import { LibraryProgress, PlexLibraryDTO, PlexMediaType, ViewMode } from '@dto/mainApi';
import { DownloadService, LibraryService, MediaService, SettingsService, SignalrService } from '@service';
import { DetailsOverview, DownloadConfirmation, MediaTable } from '#components';
import ISelection from '@interfaces/ISelection';
import {
	setMediaOverviewSort,
	useMediaOverviewBarBus,
	useMediaOverviewBarDownloadCommandBus,
	useMediaOverviewSortBus,
	useOpenControlDialog,
	listenMediaOverviewDownloadCommand,
	useCloseControlDialog,
} from '#imports';
import {
	IMediaOverviewSort,
	listenMediaOverviewOpenDetailsCommand,
	sendMediaOverviewOpenDetailsCommand,
} from '@composables/event-bus';

// region SetupFields

const router = useRouter();
const scrollDict = ref<Record<string, number>>({});
const selected = ref<ISelection>({ keys: [], allSelected: false, indexKey: 0 });

// endregion

const downloadConfirmationName = 'mediaDownloadConfirmation';
const mediaDetailsDialogName = 'mediaDetailsDialogName';
const isRefreshing = ref(false);

const server = ref<PlexServerDTO | null>(null);
const library = ref<PlexLibraryDTO | null>(null);

const libraryProgress = ref<LibraryProgress | null>(null);
const items = ref<PlexMediaSlimDTO[]>([]);

const loading = ref(true);
const showMediaOverview = ref(true);
const mediaItem = ref<PlexMediaDTO | null>(null);
const mediaViewMode = ref<ViewMode>(ViewMode.Poster);

const askDownloadMovieConfirmation = ref(false);
const askDownloadTvShowConfirmation = ref(false);
const askDownloadSeasonConfirmation = ref(false);
const askDownloadEpisodeConfirmation = ref(false);

const props = defineProps<{
	libraryId: number;
	mediaId: number;
	mediaType: PlexMediaType;
}>();

const isConfirmationEnabled = computed(() => {
	switch (props.mediaType) {
		case PlexMediaType.Movie:
			return askDownloadMovieConfirmation.value;
		case PlexMediaType.TvShow:
			return askDownloadTvShowConfirmation.value;
		case PlexMediaType.Season:
			return askDownloadSeasonConfirmation.value;
		case PlexMediaType.Episode:
			return askDownloadEpisodeConfirmation.value;
		default:
			return true;
	}
});

const changeView = (viewMode: ViewMode) => {
	let type: keyof DisplaySettingsDTO | null = null;

	switch (props.mediaType) {
		case PlexMediaType.Movie:
			type = 'movieViewMode';
			break;
		case PlexMediaType.TvShow:
			type = 'tvShowViewMode';
			break;
		default:
			Log.error('Could not set view mode for type' + props.mediaType);
	}
	if (type) {
		useSubscription(SettingsService.updateDisplaySettings(type, viewMode).subscribe());
	}
};

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
	LibraryService.refreshLibrary(props.libraryId).subscribe(() => {
		set(isRefreshing, false);
	});
}

function onRequestMedia({ page, size, refresh }: { page: number; size: number; refresh: () => void }) {
	useSubscription(
		MediaService.getMediaData(props.libraryId, page, size)
			.pipe(take(1))
			.subscribe({
				next: (mediaData) => {
					if (!mediaData) {
						Log.error(`MediaOverview => No media data for library id ${props.libraryId} was found`);
					}

					set(items, mediaData);
				},
				error: (error) => {
					Log.error(`MediaOverview => Error while server and mediaData for library id ${props.libraryId}:`, error);
				},
				complete: () => {
					if (refresh) {
						refresh();
					}
					setScrollIndexes();
				},
			}),
	);
}

function setScrollIndexes() {
	setMediaOverviewSort({ sort: 'asc', field: 'sortTitle' });
	scrollDict.value['#'] = 0;
	// Check for occurrence of title with alphabetic character
	const sortTitles = get(items).map((x) => x.sortTitle[0].toLowerCase());
	let lastIndex = 0;
	for (const letter of 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.toLowerCase()) {
		const index = sortTitles.indexOf(letter, lastIndex);
		if (index > -1) {
			get(scrollDict)[letter] = index;
			lastIndex = index;
		}
	}
}

// region Eventbus

/**
 * Listen for process download command
 */
listenMediaOverviewDownloadCommand((command) => {
	// Only show if there is more than 1 selection
	if (command.length > 0 && command.some((x) => x.mediaIds.length > 0)) {
		if (isConfirmationEnabled.value) {
			useOpenControlDialog(downloadConfirmationName, command);
		} else {
			// sendDownloadCommand(command);
		}
	}
});

listenMediaOverviewOpenDetailsCommand((mediaId: number) => {
	Log.debug('Received open details command', mediaId);
	if (!mediaId) {
		Log.error('mediaId was invalid, could not open details', mediaId);
		return;
	}
	// if (!get(mediaDetailsDialogRef)) {
	// 	Log.error('mediaDetailsDialogRef was invalid, could not open details', mediaDetailsDialogRef);
	// 	return;
	// }
	if (!router.currentRoute.value.path.includes('details')) {
		router.push({
			path: props.libraryId + '/details/' + mediaId,
		});
	}

	set(showMediaOverview, false);
	const type = props.mediaType;
	useOpenControlDialog(mediaDetailsDialogName, { mediaId, type });
});

function closeDetailsOverview() {
	router.push({
		path: '/tvshows/' + props.libraryId,
	});
	useCloseControlDialog(mediaDetailsDialogName);

	set(showMediaOverview, true);
}

useMediaOverviewBarDownloadCommandBus().on(() => {
	if (showMediaOverview.value) {
		const downloadCommand: DownloadMediaDTO = {
			plexServerId: server.value?.id ?? 0,
			plexLibraryId: props.libraryId,
			mediaIds: selected.value.keys,
			type: props.mediaType,
		};
		sendMediaOverviewDownloadCommand([downloadCommand]);
	}
});

let sortedState: IMediaOverviewSort[] = [];
useMediaOverviewSortBus().on((event) => {
	const newSortedState = [...sortedState];
	const index = newSortedState.findIndex((x) => x.field === event.field);
	if (index > -1) {
		newSortedState.splice(index, 1);
	}
	if (event.sort !== 'none') {
		newSortedState.unshift(event);
	}

	// Prevent unnecessary sorting
	if (isEqual(sortedState, newSortedState)) {
		return;
	}
	sortedState = newSortedState;
	Log.debug('new sorted state', sortedState);
	const sortedItems = orderBy(
		get(items),
		sortedState.map((x) => x.field),
		sortedState.map((x) => x.sort),
	);

	set(items, sortedItems);
});

const mediaOverViewBarBus = useMediaOverviewBarBus();
watch(selected, () => {
	mediaOverViewBarBus.emit({
		downloadButtonVisible: selected.value.keys.length > 0,
	});
});

// endregion

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
		refresh: () => {
			set(loading, false);
		},
	});

	// Get display settings
	if (props.mediaType === PlexMediaType.Movie) {
		useSubscription(
			SettingsService.getMovieViewMode().subscribe((value) => {
				set(mediaViewMode, value);
			}),
		);
	} else if (props.mediaType === PlexMediaType.TvShow) {
		useSubscription(
			SettingsService.getTvShowViewMode().subscribe((value) => {
				set(mediaViewMode, value);
			}),
		);
	}

	useSubscription(
		SettingsService.getAskDownloadMovieConfirmation().subscribe((value) => {
			set(askDownloadMovieConfirmation, value);
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadTvShowConfirmation().subscribe((value) => {
			set(askDownloadTvShowConfirmation, value);
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadSeasonConfirmation().subscribe((value) => {
			set(askDownloadSeasonConfirmation, value);
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadEpisodeConfirmation().subscribe((value) => {
			set(askDownloadEpisodeConfirmation, value);
		}),
	);

	// Setup progress bar
	useSubscription(
		SignalrService.getLibraryProgress(props.libraryId).subscribe((data) => {
			if (data) {
				set(libraryProgress, data);
				set(isRefreshing, data.isRefreshing);
				if (data.isComplete) {
					refreshLibrary();
				}
			}
		}),
	);

	useSubscription(
		LibraryService.getServerByLibraryId(props.libraryId).subscribe((serverData) => {
			if (!serverData) {
				Log.error(`MediaOverview => Server for library id ${props.libraryId} was not found`);
			}
			set(server, serverData);
		}),
	);

	useSubscription(
		LibraryService.getLibrary(props.libraryId).subscribe((libraryData) => {
			if (!libraryData) {
				Log.error(`MediaOverview => Library for library id ${props.libraryId} was not found`);
			}
			set(library, libraryData);
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

#media-container,
.media-table-container,
.detail-view-container {
	// We need a set height so we calculate the remaining content space by subtracting other component heights
	height: calc(100vh - $app-bar-height - $media-overview-bar-height);
	width: 100%;
	overflow: hidden;
}
</style>
