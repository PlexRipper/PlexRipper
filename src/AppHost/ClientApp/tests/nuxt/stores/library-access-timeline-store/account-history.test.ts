import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexLibraryPaths } from '@api-urls';
import { PlexAccessState, type PlexLibraryAccessTimelineDTO, type PlexLibraryAccessTimelineEventDTO } from '@dto';
import { generateResultDTO } from '@mock';
import { useAccountStore, useLibraryAccessTimelineStore } from '@store';

interface TimelineEventInput {
	id: number;
	accountId: number;
	accountName: string;
	createdAt: string;
	refreshRunId: string;
	state: PlexAccessState;
	serverId?: number;
	serverName?: string;
	libraryId?: number;
	libraryName?: string;
}

function createTimelineEvent(input: TimelineEventInput): PlexLibraryAccessTimelineEventDTO {
	return {
		createdAt: input.createdAt,
		id: input.id,
		plexAccountId: input.accountId,
		plexAccountName: input.accountName,
		plexLibraryId: input.libraryId ?? 8,
		plexLibraryName: input.libraryName ?? 'Shared Library',
		plexServerId: input.serverId ?? 7,
		plexServerName: input.serverName ?? 'Shared Server',
		refreshRunId: input.refreshRunId,
		state: input.state,
	};
}

function createTimeline(events: PlexLibraryAccessTimelineEventDTO[]): PlexLibraryAccessTimelineDTO {
	return { currentState: [], events };
}

describe('LibraryAccessTimelineStore account history', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should pair interleaved history by account identity even when display names match', async () => {
		// Arrange
		useAccountStore().accounts = [];
		const timelineStore = useLibraryAccessTimelineStore();
		const aGrantedAt = '2026-01-01T00:00:00.000Z';
		const bGrantedAt = '2026-01-01T01:00:00.000Z';
		const aRevokedAt = '2026-01-01T06:00:00.000Z';
		const bRevokedAt = '2026-01-01T14:00:00.000Z';
		const timeline = createTimeline([
			createTimelineEvent({
				id: 1,
				accountId: 101,
				accountName: 'Shared Account',
				createdAt: aGrantedAt,
				refreshRunId: 'run-a-grant',
				state: PlexAccessState.Granted,
			}),
			createTimelineEvent({
				id: 2,
				accountId: 202,
				accountName: 'Shared Account',
				createdAt: bGrantedAt,
				refreshRunId: 'run-b-grant',
				state: PlexAccessState.Granted,
			}),
			createTimelineEvent({
				id: 3,
				accountId: 101,
				accountName: 'Shared Account',
				createdAt: aRevokedAt,
				refreshRunId: 'run-a-revoke',
				state: PlexAccessState.Revoked,
			}),
			createTimelineEvent({
				id: 4,
				accountId: 202,
				accountName: 'Shared Account',
				createdAt: bRevokedAt,
				refreshRunId: 'run-b-revoke',
				state: PlexAccessState.Revoked,
			}),
		]);
		const timelinePath = PlexLibraryPaths.getPlexLibraryAccessTimelineEndpoint();
		mock.onGet(timelinePath).reply(200, generateResultDTO(timeline));

		// Act
		await subscribeSpyTo(timelineStore.setup()).onComplete();
		await subscribeSpyTo(timelineStore.refreshTimeline()).onComplete();

		// Assert
		expect(mock.history.get.map((request) => request.url)).toEqual([timelinePath]);
		const intervals = timelineStore.timelineIntervals;
		expect(intervals).toHaveLength(2);
		expect(intervals).toEqual(expect.arrayContaining([
			expect.objectContaining({
				accountName: 'Shared Account',
				serverName: 'Shared Server',
				libraryName: 'Shared Library',
				grantedAt: aGrantedAt,
				revokedAt: aRevokedAt,
				durationLabel: '6h',
			}),
			expect.objectContaining({
				accountName: 'Shared Account',
				serverName: 'Shared Server',
				libraryName: 'Shared Library',
				grantedAt: bGrantedAt,
				revokedAt: bRevokedAt,
				durationLabel: '13h',
			}),
		]));
		expect(new Set(intervals.map((interval) => interval.rowId)).size).toBe(2);
		expect(new Set(intervals.map((interval) => interval.id)).size).toBe(2);
	});

	test('Should leave account A active when only account B is revoked', async () => {
		// Arrange
		const timelineStore = useLibraryAccessTimelineStore();
		const aGrantedAt = '2026-02-01T00:00:00.000Z';
		const bGrantedAt = '2026-02-02T00:00:00.000Z';
		const bRevokedAt = '2026-02-02T12:00:00.000Z';
		const timeline = createTimeline([
			createTimelineEvent({
				id: 11,
				accountId: 101,
				accountName: 'Account A',
				createdAt: aGrantedAt,
				refreshRunId: 'run-a-grant',
				state: PlexAccessState.Granted,
			}),
			createTimelineEvent({
				id: 12,
				accountId: 202,
				accountName: 'Account B',
				createdAt: bGrantedAt,
				refreshRunId: 'run-b-grant',
				state: PlexAccessState.Granted,
			}),
			createTimelineEvent({
				id: 13,
				accountId: 202,
				accountName: 'Account B',
				createdAt: bRevokedAt,
				refreshRunId: 'run-b-revoke',
				state: PlexAccessState.Revoked,
			}),
		]);
		mock.onGet(PlexLibraryPaths.getPlexLibraryAccessTimelineEndpoint()).reply(200, generateResultDTO(timeline));

		// Act
		await subscribeSpyTo(timelineStore.setup()).onComplete();
		const result = subscribeSpyTo(timelineStore.refreshTimeline());
		await result.onComplete();

		// Assert
		const intervals = timelineStore.timelineIntervals;
		expect(intervals).toHaveLength(2);
		const accountAInterval = intervals.find((interval) => interval.accountName === 'Account A');
		const accountBInterval = intervals.find((interval) => interval.accountName === 'Account B');
		expect(accountAInterval).toEqual(expect.objectContaining({
			accountName: 'Account A',
			grantedAt: aGrantedAt,
			revokedAt: null,
		}));
		expect(accountBInterval).toEqual(expect.objectContaining({
			accountName: 'Account B',
			grantedAt: bGrantedAt,
			revokedAt: bRevokedAt,
			durationLabel: '12h',
		}));
		expect(new Set(intervals.map((interval) => interval.rowId)).size).toBe(2);
		expect(new Set(intervals.map((interval) => interval.id)).size).toBe(2);
	});

	test('Should preserve independent server and library boundaries when their display names match', async () => {
		// Arrange
		const timelineStore = useLibraryAccessTimelineStore();
		const boundaries = [
			{ serverId: 70, libraryId: 80, createdAt: '2026-03-01T00:00:00.000Z' },
			{ serverId: 71, libraryId: 80, createdAt: '2026-03-02T00:00:00.000Z' },
			{ serverId: 70, libraryId: 81, createdAt: '2026-03-03T00:00:00.000Z' },
		];
		const timeline = createTimeline(boundaries.map((boundary, index) => createTimelineEvent({
			...boundary,
			id: index + 21,
			accountId: 303,
			accountName: 'Shared Account',
			refreshRunId: `run-boundary-${index}`,
			state: PlexAccessState.Granted,
			serverName: 'Duplicate Server',
			libraryName: 'Duplicate Library',
		})));
		mock.onGet(PlexLibraryPaths.getPlexLibraryAccessTimelineEndpoint()).reply(200, generateResultDTO(timeline));

		// Act
		await subscribeSpyTo(timelineStore.setup()).onComplete();
		const result = subscribeSpyTo(timelineStore.refreshTimeline());
		await result.onComplete();

		// Assert
		const intervals = timelineStore.timelineIntervals;
		expect(intervals).toHaveLength(3);
		for (const boundary of boundaries) {
			expect(intervals).toEqual(expect.arrayContaining([
				expect.objectContaining({
					accountName: 'Shared Account',
					serverName: 'Duplicate Server',
					libraryName: 'Duplicate Library',
					grantedAt: boundary.createdAt,
					revokedAt: null,
				}),
			]));
		}
		expect(new Set(intervals.map((interval) => interval.serverGroupId)).size).toBe(2);
		expect(new Set(intervals.map((interval) => interval.rowId)).size).toBe(3);
		expect(new Set(intervals.map((interval) => interval.id)).size).toBe(3);
	});
});
