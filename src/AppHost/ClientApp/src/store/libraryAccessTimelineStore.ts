import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, computed, toRefs } from 'vue';
import { type Observable, of } from 'rxjs';
import { catchError, finalize, map, tap } from 'rxjs/operators';
import { cloneDeep, groupBy, orderBy, compact } from 'lodash-es';
import { plexLibraryApi } from '@api';
import {
	PlexAccessState,
	type PlexLibraryAccessCurrentStateLibraryDTO,
	type PlexLibraryAccessTimelineDTO,
	type PlexLibraryAccessTimelineEventDTO,
} from '@dto';
import { StoreNames, type ISetupResult } from '@interfaces';

export type LibraryAccessTimelineZoomPreset = '1d' | '7d' | '30d' | '90d' | 'All';

export interface LibraryAccessTimelineInterval {
	id: string;
	rowId: string;
	accountName: string;
	serverName: string;
	libraryName: string;
	grantedAt: string;
	revokedAt: string | null;
	start: Date;
	end: Date;
	durationLabel: string;
}

interface ILibraryAccessTimelineFilters {
	plexAccountId: number | null;
	plexServerId: number | null;
	plexLibraryId: number | null;
	fromUtc: string | null;
	toUtc: string | null;
}

interface ILibraryAccessTimelineStoreState {
	filters: ILibraryAccessTimelineFilters;
	timeline: PlexLibraryAccessTimelineDTO;
	isLoading: boolean;
	selectedRowId: string;
	zoomPreset: LibraryAccessTimelineZoomPreset;
}

export const useLibraryAccessTimelineStore = defineStore(StoreNames.LibraryAccessTimelineStore, () => {
	const defaultState: ILibraryAccessTimelineStoreState = {
		filters: {
			plexAccountId: null,
			plexServerId: null,
			plexLibraryId: null,
			fromUtc: null,
			toUtc: null,
		},
		timeline: {
			events: [],
			currentState: [],
		},
		isLoading: false,
		selectedRowId: '',
		zoomPreset: '30d',
	};

	const state = reactive<ILibraryAccessTimelineStoreState>(cloneDeep(defaultState));

	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: StoreNames.LibraryAccessTimelineStore, isSuccess: true });
		},
		setAccount(plexAccountId: number | null) {
			state.filters.plexAccountId = plexAccountId;
		},
		setServer(plexServerId: number | null) {
			state.filters.plexServerId = plexServerId;
		},
		setLibrary(plexLibraryId: number | null) {
			state.filters.plexLibraryId = plexLibraryId;
		},
		setDateRange(fromUtc: string | null, toUtc: string | null) {
			state.filters.fromUtc = fromUtc;
			state.filters.toUtc = toUtc;
		},
		setZoomPreset(zoomPreset: LibraryAccessTimelineZoomPreset) {
			state.zoomPreset = zoomPreset;
		},
		selectRow(rowId: string) {
			state.selectedRowId = rowId;
		},
		refreshTimeline(): Observable<PlexLibraryAccessTimelineDTO> {
			if (!state.filters.plexAccountId) {
				state.timeline = cloneDeep(defaultState.timeline);
				return of(state.timeline);
			}

			state.isLoading = true;

			return plexLibraryApi.getPlexLibraryAccessTimelineEndpoint().pipe(
				tap((result) => {
					if (result.isSuccess && result.value) {
						state.timeline = result.value;
					}
				}),
				map(() => state.timeline),
				catchError((error) => {
					Log.error('Failed to refresh library access timeline', error);
					return of(state.timeline);
				}),
				finalize(() => state.isLoading = false),
			);
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	function buildRowId(plexServerId?: number | null, plexLibraryId?: number | null): string {
		return `server-${plexServerId ?? 'unknown'}-library-${plexLibraryId ?? 'unknown'}`;
	}

	function formatDuration(start: Date, end: Date): string {
		const durationMs = Math.max(0, end.getTime() - start.getTime());
		const days = Math.floor(durationMs / 86400000);
		if (days > 0) {
			return `${days}d`;
		}

		const hours = Math.floor(durationMs / 3600000);
		return `${hours}h`;
	}

	function buildIntervals(): LibraryAccessTimelineInterval[] {
		const now = new Date();
		const groupedEvents = groupBy(orderBy(state.timeline.events, (event) => event.createdAt, 'asc'), (event) => buildRowId(event.plexServerId, event.plexLibraryId));

		return Object.entries(groupedEvents).flatMap(([rowId, events]) => {
			const intervals: LibraryAccessTimelineInterval[] = [];
			let openGrant: PlexLibraryAccessTimelineEventDTO | null = null;

			for (const event of events) {
				if (event.state === PlexAccessState.Granted) {
					openGrant = event;
					continue;
				}

				if ((event.state === PlexAccessState.Revoked) && openGrant) {
					const start = new Date(openGrant.createdAt);
					const end = new Date(event.createdAt);
					if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
						openGrant = null;
						continue;
					}
					intervals.push({
						id: `${rowId}-${openGrant.refreshRunId}-${event.refreshRunId}`,
						rowId,
						accountName: openGrant.plexAccountName,
						serverName: openGrant.plexServerName ?? '',
						libraryName: openGrant.plexLibraryName ?? '',
						grantedAt: openGrant.createdAt,
						revokedAt: event.createdAt,
						start,
						end,
						durationLabel: formatDuration(start, end),
					});
					openGrant = null;
				}
			}

			if (openGrant) {
				const start = new Date(openGrant.createdAt);
				if (Number.isNaN(start.getTime())) {
					return intervals;
				}
				intervals.push({
					id: `${rowId}-${openGrant.refreshRunId}-active`,
					rowId,
					accountName: openGrant.plexAccountName,
					serverName: openGrant.plexServerName ?? '',
					libraryName: openGrant.plexLibraryName ?? '',
					grantedAt: openGrant.createdAt,
					revokedAt: null,
					start,
					end: now,
					durationLabel: formatDuration(start, now),
				});
			}

			return intervals;
		});
	}

	const getters = {
		timelineIntervals: computed((): LibraryAccessTimelineInterval[] => buildIntervals()),
		currentStateRows: computed((): PlexLibraryAccessCurrentStateLibraryDTO[] => compact(state.timeline.currentState.flatMap((server) => server.libraries))),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useLibraryAccessTimelineStore, import.meta.hot));
}
