import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateResultDTO } from '@mock';
import { DebugPaths } from '@api-urls';
import { useLogsStore, useSignalrStore } from '@store';
import { LogSeverity, type LiveLogEventDTO } from '@dto';

describe('LogsStore.refreshLogs()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should merge shared event IDs once and retain separate same-text events across repeated snapshots', async () => {
		// Arrange
		const cached: LiveLogEventDTO = {
			sequence: 100,
			timestamp: '2026-10-18T12:00:00Z',
			level: LogSeverity.Information,
			message: 'Library sync started',
		};
		const firstLive: LiveLogEventDTO = { ...cached, sequence: 101, message: 'Library sync finished' };
		const secondLive: LiveLogEventDTO = { ...firstLive, sequence: 102 };
		let snapshot = [cached];
		mock.onGet(DebugPaths.getAllLogsEndpoint()).reply(() => [200, generateResultDTO(snapshot)]);
		const store = useLogsStore();
		const signalrStore = useSignalrStore();
		await subscribeSpyTo(store.setup()).onComplete();
		expect(store.getLogs).toEqual([cached]);
		signalrStore.logEventSubject.next(firstLive);
		signalrStore.logEventSubject.next(secondLive);
		expect(store.getLogs).toEqual([cached, firstLive, secondLive]);
		snapshot = [cached, firstLive];

		// Act
		await subscribeSpyTo(store.refreshLogs()).onComplete();
		await subscribeSpyTo(store.refreshLogs()).onComplete();

		// Assert
		expect(store.getLogs).toEqual([cached, firstLive, secondLive]);
		expect(store.getLogs.map((entry) => entry.sequence)).toEqual([100, 101, 102]);
		expect(mock.history.get.filter((request) => request.url === DebugPaths.getAllLogsEndpoint())).toHaveLength(3);

		// A later snapshot catches up with the event that arrived after the earlier snapshot.
		snapshot = [cached, firstLive, secondLive];
		await subscribeSpyTo(store.refreshLogs()).onComplete();
		expect(store.getLogs).toEqual([cached, firstLive, secondLive]);
	});
});
