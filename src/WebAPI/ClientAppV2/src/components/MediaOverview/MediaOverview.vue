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
	<template v-if="!loading">
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
					@refresh-library="refreshLibrary"
					@download="processDownloadCommand([])" />
				<!--	Data table display	-->
				<div ref="mediaContainerRef" class="media-container">
					<q-row>
						<q-col cols="grow media-table-container">
							<div>
								<template v-if="mediaViewMode === ViewMode.Table">
									<MediaTable
										ref="overviewMediaTableRef"
										row-key="id"
										:selection="selected"
										:media-type="mediaType"
										:rows="items"
										:library="library"
										:scroll-dict="scrollDict"
										:style="getHeightStyle"
										@download="processDownloadCommand"
										@selection="selected = $event"
										@request-media="onRequestMedia($event)" />
								</template>

								<!-- Poster display-->
								<template v-else>
									<QScrollArea ref="mediaContainerScrollbarRef" class="fit" @scroll="setScrollPosition($event)">
										<poster-table
											:library-id="libraryId"
											:media-type="mediaType"
											:items="items"
											@download="processDownloadCommand"
											@open-details="openDetails" />
									</QScrollArea>
								</template>
							</div>

							<!--	Overlay with details of the media	-->
						</q-col>
						<!-- Alphabet Navigation-->
						<alphabet-navigation :items="items" :scroll-dict="scrollDict" @scroll-to="scrollToIndex($event)" />
					</q-row>
				</div>
			</q-col>
		</q-row>
	</template>
	<template v-else>
		<q-row justify="center">
			<q-col cols="auto">
				<h1>{{ $t('components.media-overview.no-data') }}</h1>
			</q-col>
		</q-row>
	</template>

	<!-- Media detail view	-->
	<q-dialog
		v-model="fixed"
		seamless
		maximized
		class="media-details-dialog"
		transition-show="slide-up"
		transition-hide="slide-down"
		@show="onOpenDetails">
		<div :style="getDetailsStyle">
			<QScrollArea class="fit">
				<DetailsOverview ref="detailsOverviewRef" @close="closeDetailsOverview" @media-item="mediaItem = $event" />
			</QScrollArea>
		</div>
	</q-dialog>

	<!--		Download confirmation dialog	-->
	<DownloadConfirmation ref="downloadConfirmationRef" :items="items" @download="sendDownloadCommand" />
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, defineProps, computed } from 'vue';
import { useElementBounding, useEventBus } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { useRoute, useRouter } from 'vue-router';
import { take } from 'rxjs/operators';
import { QScrollArea } from 'quasar';
import type { DisplaySettingsDTO, DownloadMediaDTO, PlexMediaDTO, PlexMediaSlimDTO, PlexServerDTO } from '@dto/mainApi';
import { LibraryProgress, PlexLibraryDTO, PlexMediaType, ViewMode } from '@dto/mainApi';
import { DownloadService, LibraryService, MediaService, SettingsService, SignalrService } from '@service';
import { DetailsOverview, DownloadConfirmation, MediaTable } from '#components';
import ISelection from '@interfaces/ISelection';

// region SetupFields

const router = useRouter();
const route = useRoute();
const mediaContainerRef = ref(null);
const mediaContainerSize = useElementBounding(mediaContainerRef);
const processDownloadCommandBus = useEventBus<DownloadMediaDTO[]>('processDownloadCommand');
const scrollDict = ref<Record<string, number>>({});
const selected = ref<ISelection>({ keys: [], allSelected: false, indexKey: 0 });

// endregion

const fixed = ref(false);

const activeAccountId = ref(0);
const isRefreshing = ref(false);

const server = ref<PlexServerDTO | null>(null);
const library = ref<PlexLibraryDTO | null>(null);

const libraryProgress = ref<LibraryProgress | null>(null);
const items = ref<PlexMediaSlimDTO[]>([]);

const loading = ref(true);
const showMediaOverview = ref(true);
const mediaItem = ref<PlexMediaDTO | null>(null);
const mediaViewMode = ref<ViewMode>(ViewMode.Poster);
const currentMediaItemId = ref<number | null>(null);

const downloadConfirmationRef = ref<InstanceType<typeof DownloadConfirmation> | null>(null);
const detailsOverviewRef = ref<InstanceType<typeof DetailsOverview> | null>(null);
const mediaContainerScrollbarRef = ref<InstanceType<typeof QScrollArea> | null>(null);
const overviewMediaTableRef = ref<InstanceType<typeof MediaTable> | null>(null);

const scrollPosition = ref(0);

const props = defineProps<{
	libraryId: number;
	mediaType: PlexMediaType;
}>();

const getPercentage = computed(() => libraryProgress.value?.percentage ?? -1);
const getHeightStyle = computed(() => {
	const height = mediaContainerSize.height.value;
	return {
		height: height + 'px !important',
		minHeight: height + 'px !important',
		maxHeight: height + 'px !important',
	};
});

const getDetailsStyle = computed(() => {
	const width = mediaContainerSize.width.value;
	const height = mediaContainerSize.height.value;

	return {
		width: width + 'px !important',
		minWidth: width + 'px !important',
		maxWidth: width + 'px !important',
		height: height + 'px !important',
		minHeight: height + 'px !important',
		maxHeight: height + 'px !important',
	};
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

const scrollToIndex = (letter: string) => {
	if (overviewMediaTableRef.value) {
		overviewMediaTableRef.value.scrollToIndex(letter);
		return;
	}
	Log.error('overviewMediaTableRef.value is null');
};

const setScrollPosition = (info: any) => {
	if (showMediaOverview.value) {
		scrollPosition.value = info.verticalPosition;
	}
};

const resetProgress = (isRefreshingValue: boolean) => {
	isRefreshing.value = isRefreshingValue;

	libraryProgress.value = {
		id: props.libraryId,
		percentage: 0,
		received: 0,
		total: 0,
		isRefreshing: isRefreshingValue,
		isComplete: false,
		timeStamp: '',
	};
};

const processDownloadCommand = (downloadMediaCommand: DownloadMediaDTO[]) => {
	Log.info('processDownloadCommand', downloadMediaCommand);

	if (downloadMediaCommand.length > 0) {
		downloadConfirmationRef.value.openDialog(downloadMediaCommand, items.value);
	}
	// if (overviewMediaTableRef.value) {
	// 	downloadConfirmationRef.value?.openDialog(overviewMediaTableRef.value.createDownloadCommands());
	// } else {
	// 	Log.error('overviewMediaTableRef was invalid', overviewMediaTableRef.value);
	// }
};

const sendDownloadCommand = (downloadMediaCommand: DownloadMediaDTO[]) => {
	DownloadService.downloadMedia(downloadMediaCommand);
};

const openDetails = (mediaId: number) => {
	if (!router.currentRoute.value.path.includes('details')) {
		router.push({
			path: props.libraryId + '/details/' + mediaId,
		});
	}
	currentMediaItemId.value = mediaId;
	fixed.value = true;
};

const onOpenDetails = () => {
	if (detailsOverviewRef.value) {
		detailsOverviewRef.value.openDetails(currentMediaItemId.value ?? 0, props.mediaType);
	} else {
		Log.error('detailsOverview was invalid', detailsOverviewRef.value);
	}
	showMediaOverview.value = false;
};

const closeDetailsOverview = () => {
	router.push({
		path: '/tvshows/' + props.libraryId,
	});
	showMediaOverview.value = true;
	fixed.value = false;
};

const refreshLibrary = () => {
	isRefreshing.value = true;
	resetProgress(true);
	LibraryService.refreshLibrary(props.libraryId).subscribe((data) => {
		setLibrary(data);
		isRefreshing.value = false;
	});
};

const setLibrary = (data: PlexLibraryDTO | null) => {
	if (data) {
		library.value = data;
		switch (props.mediaType) {
			case PlexMediaType.Movie:
				items.value = library.value?.movies ?? [];
				break;
			case PlexMediaType.TvShow:
				items.value = library.value?.tvShows ?? [];
				break;
		}
	}
};

const onRequestMedia = ({ page, size, refresh }: { page: number; size: number; refresh: () => void }) => {
	Log.info('onRequestMedia', page, size);
	useSubscription(
		MediaService.getMediaData(props.libraryId, page, size)
			.pipe(take(1))
			.subscribe({
				next: (mediaData) => {
					if (!mediaData) {
						Log.error(`MediaOverview => No media data for library id ${props.libraryId} was found`);
					}
					items.value = mediaData;
				},
				error: (error) => {
					Log.error(`MediaOverview => Error while server and mediaData for library id ${props.libraryId}:`, error);
				},
				complete: () => {
					if (refresh) {
						refresh();
					}
					setScrollIndexes(items.value);
				},
			}),
	);
};

const setScrollIndexes = (items: PlexMediaSlimDTO[]) => {
	scrollDict.value['#'] = 0;
	// Check for occurrence of title with alphabetic character
	for (const letter of 'ABCDEFGHIJKLMNOPQRSTUVWXYZ') {
		const index = items.findIndex((x) => x.sortTitle.startsWith(letter));
		if (index > -1) {
			scrollDict.value[letter] = index;
		}
	}
	Log.info('setScrollIndexes', scrollDict.value);
};

onBeforeMount(() => {
	const mediaId = +route.params.tvShowId;
	if (mediaId) {
		openDetails(mediaId);
	}
});

onMounted(() => {
	resetProgress(false);
	isRefreshing.value = false;

	if (!props.libraryId) {
		Log.error('Library id was not provided');
		return;
	}

	// Initial data load
	onRequestMedia({
		page: 0,
		size: 0,
		refresh: () => {
			loading.value = false;
		},
	});

	processDownloadCommandBus.on((event) => processDownloadCommand(event));

	// Get Active account id
	useSubscription(SettingsService.getActiveAccountId().subscribe((id) => (activeAccountId.value = id)));

	// Get display settings
	if (props.mediaType === PlexMediaType.Movie) {
		useSubscription(
			SettingsService.getMovieViewMode().subscribe((value) => {
				mediaViewMode.value = value;
			}),
		);
	} else if (props.mediaType === PlexMediaType.TvShow) {
		useSubscription(
			SettingsService.getTvShowViewMode().subscribe((value) => {
				mediaViewMode.value = value;
			}),
		);
	}

	// Setup progress bar
	useSubscription(
		SignalrService.getLibraryProgress(props.libraryId).subscribe((data) => {
			if (data) {
				libraryProgress.value = data;
				isRefreshing.value = data.isRefreshing;
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
			server.value = serverData;
		}),
	);

	useSubscription(
		LibraryService.getLibrary(props.libraryId).subscribe((libraryData) => {
			if (!libraryData) {
				Log.error(`MediaOverview => Library for library id ${props.libraryId} was not found`);
			}
			library.value = libraryData;
		}),
	);
});
</script>

<style lang="scss">
.media-container,
.media-table-container,
.detail-view-container {
	height: calc(100vh - 85px - 48px);
	width: 100%;
}

.media-details-dialog {
	.q-dialog__inner {
		top: auto !important;
		left: auto !important;
		bottom: 0 !important;
		right: 0 !important;
	}
}
</style>
