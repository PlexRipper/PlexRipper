import Log from 'consola';
import { cloneDeep, isNumber, sortBy, uniqueId } from 'lodash-es';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { computed, markRaw, reactive, toRefs } from 'vue';
import { get } from '@vueuse/core';
import {
	type MediaQueryFilterDTO,
	type PlexMediaMetadataDTO,
	type PlexMediaSlimDTO,
	type PlexMediaStatisticsDTO,
	PlexMediaType,
	VideoQuality,
	ViewMode,
} from '@dto';
import type { IMediaOverviewSort } from '@composables/event-bus';
import { MediaSortField, SortDirection } from '@enums';
import { type IMetaDataMediaFilter, type ISelection, type ISortOption, StoreNames } from '@interfaces';
import { plexLibraryApi, plexMediaApi } from '@api';
import { finalize, map, switchMap, takeUntil, tap } from 'rxjs/operators';
import { defer, forkJoin, type Observable, of, Subject } from 'rxjs';
import { useLibraryStore, useSettingsStore } from '@store';
import {
	buildFlexSortDsl,
	DSLBuilder,
	getVideoQualityColor,
	translateVideoQuality,
} from '@composables';
import { useSubscription } from '@vueuse/rxjs';
import { useI18n } from 'vue-i18n';

interface IAvailableMetadataIds {
	roles?: number[];
	countries?: number[];
	genres?: number[];
	qualities?: number[];
}

interface IMediaOverviewStoreState {
	libraryId: number;
	loadedPages: number[];
	pageSize: number;
	totalCount: number;
	itemsLength: number;
	sortedState: IMediaOverviewSort;
	scrollDict: Map<string, number>;
	selection: ISelection;
	downloadButtonVisible: boolean;
	filterQuery: string;
	lastMediaItemViewed: PlexMediaSlimDTO | null;
	loading: boolean;
	isDetailView: boolean;
	allMovieCount: number;
	allTvShowCount: number;
	allSeasonCount: number;
	allEpisodeCount: number;
	allFileSize: number;
	metadata: IMetaDataMediaFilter;
	metadataList: PlexMediaMetadataDTO;
	availableRoleIds: number[];
	availableCountryIds: number[];
	availableGenreIds: number[];
	availableQualityIds: number[];
	// Meant to update to signify an reactive change in the mediaPages
	mediaPagesVersion: number;
	currentScrollIndex: number;
}

export const useMediaOverviewStore = defineStore(StoreNames.MediaOverviewStore, () => {
	const defaultState: IMediaOverviewStoreState = {
		libraryId: 0,
		loadedPages: [],
		pageSize: 100,
		totalCount: 0,
		itemsLength: 0,
		sortedState: { field: MediaSortField.Title, sort: SortDirection.Asc },
		scrollDict: new Map<string, number>([['#', 0]]),
		selection: { keys: [], allSelected: false, indexKey: 0 },
		downloadButtonVisible: false,
		filterQuery: '',
		lastMediaItemViewed: null,
		loading: false,
		isDetailView: false,
		allMovieCount: 0,
		allTvShowCount: 0,
		allSeasonCount: 0,
		allEpisodeCount: 0,
		allFileSize: 0,
		metadata: {
			countryId: 0,
			roleId: 0,
			genreId: 0,
			quality: VideoQuality.None,
		},
		metadataList: {
			mediaCount: 0,
			roleCount: 0,
			countryCount: 0,
			genreCount: 0,
			qualityCount: 0,
			roles: [],
			countries: [],
			genres: [],
			qualities: [],
		},
		availableRoleIds: [],
		availableCountryIds: [],
		availableGenreIds: [],
		availableQualityIds: [],
		mediaPagesVersion: 0,
		currentScrollIndex: 0,
	};

	const state = reactive<IMediaOverviewStoreState>(cloneDeep(defaultState));
	const settingsStore = useSettingsStore();
	const libraryStore = useLibraryStore();
	const mediaPages = new Map<number, readonly PlexMediaSlimDTO[]>();
	const pendingPages = new Set<number>();

	// Subject to cancel in-flight requests when switching libraries
	const cancelSubject$ = new Subject<void>();

	const actions = {
		cancelPendingRequests() {
			Log.debug('Cancelling pending media requests');
			cancelSubject$.next();
			state.loading = false;
		},
		initializeLibrary(libraryId: number): Observable<PlexMediaStatisticsDTO | null> {
			// Cancel any in-flight requests first
			actions.cancelPendingRequests();

			// Update state
			state.libraryId = libraryId;
			state.filterQuery = '';
			state.isDetailView = false;

			Log.debug('Initializing library', { libraryId, mediaType: get(getters.getMediaType) });

			// Clear filters and sorting
			actions.clearMetaDataFilter();
			actions.clearSort();

			// Load data for the library
			return actions.requestMedia();
		},
		refreshMetaData() {
			return plexLibraryApi.getLibraryMediaMetadata(state.libraryId, { mediaType: get(getters.getMediaType) }).pipe(
				takeUntil(cancelSubject$),
				tap((result) => {
					if (result.isSuccess && result.value) {
						return state.metadataList = result.value;
					}
				}),
			);
		},
		buildFlexQueryParams(page: number, size: number): MediaQueryFilterDTO {
			const filterQuery = state.filterQuery.trim().toLowerCase();

			return {
				filterOwnedMedia: settingsStore.generalSettings.hideMediaFromOwnedServers,
				filterOfflineMedia: settingsStore.generalSettings.hideMediaFromOfflineServers,
				mediaType: get(getters.getMediaType),
				plexLibraryId: state.libraryId,
				page,
				pageSize: size,
				filter: DSLBuilder()
					.when(filterQuery, (x) => x.contains('SearchTitle', filterQuery))
					.when((state.metadata.countryId ?? 0) > 0, (x) => x.where('Countries:any:Id', 'eq', state.metadata.countryId ?? 0))
					.when((state.metadata.roleId ?? 0) > 0, (x) => x.where('Actors:any:Id', 'eq', state.metadata.roleId ?? 0))
					.when((state.metadata.genreId ?? 0) > 0, (x) => x.where('Genres:any:Id', 'eq', state.metadata.genreId ?? 0))
					.when(state.metadata.quality !== VideoQuality.None, (x) => x.eq('MediaDataList:any:Quality', state.metadata.quality ?? 0))
					.build(),
				sort: buildFlexSortDsl([
					{
						field: state.sortedState.field,
						direction: state.sortedState.sort === SortDirection.Desc ? 'desc' : 'asc',
					},
				]),
			};
		},
		refreshAllLibraryMediaByType(page: number = 1, size: number = state.pageSize): Observable<PlexMediaStatisticsDTO | null> {
			const queryParams = actions.buildFlexQueryParams(page, size);
			return plexMediaApi.getAllMediaByTypeEndpoint(queryParams).pipe(
				takeUntil(cancelSubject$),
				map(({ isSuccess, value }): PlexMediaStatisticsDTO | null => {
					if (isSuccess && value) {
						return value;
					}
					return null;
				}),
			);
		},
		requestMedia(): Observable<PlexMediaStatisticsDTO | null> {
			if (state.loading) {
				Log.debug('Request already in progress, skipping');
				return of(null);
			}

			state.loading = true;
			Log.debug('Starting media request', { libraryId: state.libraryId, mediaType: get(getters.getMediaType) });

			return forkJoin([
				actions.refreshMetaData(),
				defer(() =>
					state.libraryId > 0
						? libraryStore.refreshLibrary(state.libraryId)
						: of(null),
				).pipe(takeUntil(cancelSubject$)),
			]).pipe(
				takeUntil(cancelSubject$),
				switchMap(() =>
					defer(() => actions.refreshAllLibraryMediaByType(1, state.pageSize)).pipe(
						takeUntil(cancelSubject$),
						tap((data) => {
							actions.setMedia(data);
						}),
					),
				),
				tap({
					next: () => {
						state.loading = false;
						Log.debug('Media request completed successfully');
					},
					error: (err) => {
						state.loading = false;
						Log.error('Media request failed', err);
					},
					complete: () => {
						// Handle cancellation
						if (state.loading) {
							state.loading = false;
							Log.debug('Media request was cancelled');
						}
					},
				}),
			);
		},
		requestMediaPage(page: number, size: number = state.pageSize): Observable<PlexMediaStatisticsDTO | null> {
			if (state.loadedPages.includes(page) || pendingPages.has(page)) {
				return of(null);
			}

			pendingPages.add(page);
			return actions.refreshAllLibraryMediaByType(page, size).pipe(
				tap((data) => {
					if (data) {
						actions.mergeMediaPage(data);
					}
				}),
				finalize(() => pendingPages.delete(page)),
			);
		},
		mergeMediaPage(data: PlexMediaStatisticsDTO) {
			const availableMetadataIds = data as PlexMediaStatisticsDTO & IAvailableMetadataIds;
			const frozenPage = markRaw(Object.freeze(data.mediaList));

			mediaPages.set(data.page, frozenPage);
			state.mediaPagesVersion++;
			state.loadedPages = [...new Set([...state.loadedPages, data.page])].sort((a, b) => a - b);
			state.itemsLength = state.loadedPages.reduce((total, page) => total + (mediaPages.get(page)?.length ?? 0), 0);
			state.totalCount = data.totalCount || Math.max(data.mediaCount, state.itemsLength);

			state.allMovieCount = data.totalMovieCount;
			state.allTvShowCount = data.totalTvShowCount;
			state.allSeasonCount = data.totalSeasonCount;
			state.allEpisodeCount = data.totalEpisodeCount;
			state.allFileSize = data.totalMediaSize;
			state.availableRoleIds = availableMetadataIds.roles ?? [];
			state.availableCountryIds = availableMetadataIds.countries ?? [];
			state.availableGenreIds = availableMetadataIds.genres ?? [];
			state.availableQualityIds = availableMetadataIds.qualities ?? [];
			state.scrollDict = new Map((data.navigationIndexes ?? []).map((x) => [x.label, x.index]));
		},
		setMedia(data: PlexMediaStatisticsDTO | null) {
			mediaPages.clear();
			state.mediaPagesVersion++;
			state.loadedPages = [];
			pendingPages.clear();
			state.totalCount = 0;
			state.itemsLength = 0;

			if (data) {
				actions.mergeMediaPage(data);
			} else {
				state.allMovieCount = 0;
				state.allTvShowCount = 0;
				state.allSeasonCount = 0;
				state.allEpisodeCount = 0;
				state.allFileSize = 0;
				state.availableRoleIds = [];
				state.availableCountryIds = [];
				state.availableGenreIds = [];
				state.availableQualityIds = [];
				state.scrollDict = new Map<string, number>([['#', 0]]);
			}
		},
		getPageForIndex(index: number): number {
			return Math.floor(index / state.pageSize) + 1;
		},
		requestRange(startIndex: number, endIndex: number): Observable<(PlexMediaStatisticsDTO | null)[]> {
			const firstPage = actions.getPageForIndex(Math.max(0, startIndex));
			const lastPage = actions.getPageForIndex(Math.max(0, endIndex));
			const requests: Observable<PlexMediaStatisticsDTO | null>[] = [];

			for (let page = firstPage; page <= lastPage; page++) {
				if (!state.loadedPages.includes(page)) {
					requests.push(actions.requestMediaPage(page));
				}
			}

			return requests.length ? forkJoin(requests) : of([]);
		},
		requestAroundIndex(index: number): Observable<(PlexMediaStatisticsDTO | null)[]> {
			return actions.requestRange(index - 20, index + 50);
		},
		setMetaData({
			countryId,
			roleId,
			genreId,
			quality,
		}: Partial<IMetaDataMediaFilter>): Observable<PlexMediaStatisticsDTO | null> {
			if (isNumber(countryId)) {
				state.metadata.countryId = countryId;
			}

			if (isNumber(roleId)) {
				state.metadata.roleId = roleId;
			}

			if (isNumber(genreId)) {
				state.metadata.genreId = genreId;
			}

			if (quality) {
				state.metadata.quality = quality;
			}

			return actions.requestMedia();
		},
		unsetMetaData(key: keyof IMetaDataMediaFilter): Observable<PlexMediaStatisticsDTO | null> {
			switch (key) {
				case 'countryId':
				case 'roleId':
				case 'genreId':
					state.metadata[key] = 0;
					break;
				case 'quality':
					state.metadata[key] = VideoQuality.None;
					break;
			}

			return actions.requestMedia();
		},
		clearMetaDataFilter() {
			state.metadata = {
				countryId: 0,
				roleId: 0,
				genreId: 0,
				quality: VideoQuality.None,
			};
		},
		changeAllMediaOverviewType(mediaType: PlexMediaType) {
			settingsStore.displaySettings.allOverviewViewMode = mediaType;
			useSubscription(actions.requestMedia().subscribe());
		},
		setSelection(selection: ISelection) {
			state.selection = selection;
		},
		setSelectionRange(min: number, max: number) {
			actions.setSelection({
				indexKey: state.selection.indexKey,
				keys: get(getters.getMediaItems)
					.filter((x) => x && x.sortIndex >= min && x.sortIndex <= max)
					.map((x) => x.id),
				allSelected: false,
			} as ISelection);
		},
		setRootSelected(value: boolean) {
			actions.setSelection({
				indexKey: state.selection?.indexKey ?? 0,
				keys: value ? get(getters.getMediaItems).map((x) => x.id) : [],
				allSelected: value,
			} as ISelection);
		},
		clearSort() {
			state.sortedState = { field: MediaSortField.Title, sort: SortDirection.Asc };
		},
		setCurrentScrollIndex(scrollIndex: number) {
			if (scrollIndex < 0) {
				return;
			}

			state.currentScrollIndex = scrollIndex;
		},
		setFilterQuery(query: string): Observable<PlexMediaStatisticsDTO | null> {
			state.filterQuery = query;
			return actions.requestMedia();
		},
		clearFilter(): Observable<PlexMediaStatisticsDTO | null> {
			state.filterQuery = '';
			return actions.requestMedia();
		},
		toggleSortMedia(field: MediaSortField) {
			if (state.sortedState.field === field) {
				state.sortedState.sort = state.sortedState.sort === SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc;
			} else {
				state.sortedState = { field, sort: SortDirection.Asc };
			}

			useSubscription(actions.requestMedia().subscribe());
		},
		sortMedia(event: IMediaOverviewSort) {
			Log.debug('Setting media sort state', event);
			state.sortedState = event;
		},
		$reset() {
			mediaPages.clear();
			pendingPages.clear();
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	const getters = {
		hasSelectedMedia: computed((): boolean => {
			return state.selection.keys.length > 0;
		}),

		hasNoSearchResults: computed((): boolean => {
			return state.filterQuery != '' && get(getters.getMediaItems).length === 0;
		}),
		hasNoFilterResults: computed((): boolean => {
			return state.metadataList.mediaCount > 0 && get(getters.getMediaItems).length === 0;
		}),
		allMediaMode: computed(() => state.libraryId === 0),
		library: computed(() => libraryStore.getLibrary(state.libraryId)),
		getMediaItems: computed((): Readonly<PlexMediaSlimDTO[]> => {
			return Array.from(mediaPages.values()).flat();
		}),
		getMediaItemsForRange: (start: number, end: number): Readonly<PlexMediaSlimDTO[]> => {
			const normalizedStart = Math.max(0, Math.floor(start));
			const normalizedEnd = Math.max(normalizedStart, Math.floor(end));
			const result: PlexMediaSlimDTO[] = [];

			for (let index = normalizedStart; index < normalizedEnd; index++) {
				const page = actions.getPageForIndex(index);
				const pageItems = mediaPages.get(page);

				if (!pageItems) {
					continue;
				}

				const indexInPage = index - ((page - 1) * state.pageSize);
				const mediaItem = pageItems[indexInPage];
				if (mediaItem) {
					result.push(mediaItem);
				}
			}

			return result;
		},
		getMediaViewMode: computed((): ViewMode => {
			switch (get(getters.getMediaType)) {
				case PlexMediaType.Movie:
					return settingsStore.displaySettings.movieViewMode;
				case PlexMediaType.TvShow:
					return settingsStore.displaySettings.tvShowViewMode;
				default:
					return ViewMode.Poster;
			}
		}),
		showSelectionButton: computed((): boolean => {
			return get(getters.getMediaViewMode) === ViewMode.Table && !state.isDetailView;
		}),
		showDownloadButton: computed((): boolean => {
			return state.downloadButtonVisible || (get(getters.hasSelectedMedia) && get(getters.getMediaViewMode) === ViewMode.Table);
		}),
		isRootSelected: computed((): boolean | null => {
			if (state.selection?.keys.length === state.itemsLength) {
				return true;
			}

			if (state.selection?.keys.length === 0) {
				return false;
			}

			return null;
		}),
		getActiveSort: computed((): IMediaOverviewSort => {
			return state.sortedState;
		}),
		getMediaType: computed((): PlexMediaType => state.libraryId === 0 ? settingsStore.displaySettings.allOverviewViewMode : libraryStore.getLibrary(state.libraryId)?.type ?? PlexMediaType.None),
		getIsSorted: computed((): boolean => {
			if (state.sortedState.sort === SortDirection.NoSort) {
				return false;
			}
			return !(state.sortedState.field === MediaSortField.Title && state.sortedState.sort === SortDirection.Asc);
		}),
		getSortOptions: (): ISortOption[] => {
			const { t } = useI18n({ useScope: 'global' });
			const options: ISortOption[] = [
				{ field: MediaSortField.Title, label: t('general.sort.title'), direction: SortDirection.NoSort },
				{ field: MediaSortField.Year, label: t('general.sort.year'), direction: SortDirection.NoSort },
				{ field: MediaSortField.AddedAt, label: t('general.sort.added-at'), direction: SortDirection.NoSort },
				{ field: MediaSortField.UpdatedAt, label: t('general.sort.updated-at'), direction: SortDirection.NoSort },
				{ field: MediaSortField.Duration, label: t('general.sort.duration'), direction: SortDirection.NoSort },
				{ field: MediaSortField.MediaSize, label: t('general.sort.size'), direction: SortDirection.NoSort },
				{ field: MediaSortField.Quality, label: t('general.sort.quality'), direction: SortDirection.NoSort },
			];

			for (const option of options) {
				if (option.field === state.sortedState.field) {
					option.direction = state.sortedState.sort;
				}
			}

			return options;
		},
		getGenres: computed(() => sortBy(
			state.metadataList.genres.filter((x) => state.availableGenreIds.includes(x.id)),
			(x) => x.name,
		)),
		getRoles: computed(() => sortBy(
			state.metadataList.roles.filter((x) => state.availableRoleIds.includes(x.id)),
			(x) => x.name,
		)),
		getCountries: computed(() => sortBy(
			state.metadataList.countries.filter((x) => state.availableCountryIds.includes(x.id)),
			(x) => x.name,
		)),
		getQualities: computed(() => state.metadataList.qualities.filter((x) => {
			return state.availableQualityIds.includes(x.id);
		})),
		getFilterChips: computed(() => {
			const result: { text: string; key: keyof IMetaDataMediaFilter; color?: string; id: string }[] = [];

			if (state.metadata.countryId > 0) {
				result.push({
					text: state.metadataList.countries.find((x) => x.id === state.metadata.countryId)?.name ?? '',
					key: 'countryId',
					id: uniqueId(),
				});
			}

			if (state.metadata.roleId > 0) {
				result.push({
					text: state.metadataList.roles.find((x) => x.id === state.metadata.roleId)?.name ?? '',
					key: 'roleId',
					id: uniqueId(),
				});
			}

			if (state.metadata.genreId > 0) {
				result.push({
					text: state.metadataList.genres.find((x) => x.id === state.metadata.genreId)?.name ?? '',
					key: 'genreId',
					id: uniqueId(),
				});
			}

			if (state.metadata.quality != VideoQuality.None) {
				const quality = state.metadataList.qualities.find((x) => x.quality === state.metadata.quality)?.quality;
				result.push({
					text: translateVideoQuality(quality),
					key: 'quality',
					color: getVideoQualityColor(quality),
					id: uniqueId(),
				});
			}

			return result;
		}),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaOverviewStore, import.meta.hot));
}
