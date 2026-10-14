import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { HubConnectionState } from '@microsoft/signalr';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { useSignalrStore, useLibraryStore } from '@store';
import { PlexLibraryPaths } from '@api/api-paths';
import { LibrarySyncJobStatus } from '@dto';
import { generateLibrarySyncJobQueue, generateResultDTO } from '@mock';
import { flushPromises } from '@vue/test-utils';

type HubName = 'progress' | 'download' | 'notifications';

type ReconnectedHandler = () => void;

interface MockHub {
	state: HubConnectionState;
	handlers: Map<string, (data: unknown) => void>;
	on: (event: string, callback: (data: unknown) => void) => void;
	onreconnected: (callback: ReconnectedHandler) => void;
	reconnected?: ReconnectedHandler;
	start: () => Promise<void>;
}
const { hubConnections } = vi.hoisted(() => ({
	hubConnections: new Map<HubName, MockHub>(),
}));

vi.mock('cypress-signalr-mock', () => ({
	useCypressSignalRMock: (hubName: string) => hubConnections.get(hubName as HubName),
}));

describe('SignalrStore connections', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		mock.onGet('/api/Download').reply(200, {
			isSuccess: true,
			errors: [],
			successes: [],
			statusCode: 200,
			value: [],
		});
		setActivePinia(createPinia());
		hubConnections.clear();
		hubConnections.set('progress', createHub());
		hubConnections.set('download', createHub());
		hubConnections.set('notifications', createHub());
	});

	test('Should start all hub connections when setup is run and hubs are disconnected', async () => {
		// Arrange
		const signalrStore = useSignalrStore();
		const progressHub = hubConnections.get('progress')!;
		const downloadHub = hubConnections.get('download')!;
		const notificationsHub = hubConnections.get('notifications')!;

		// Act
		await subscribeSpyTo(signalrStore.setup()).onComplete();

		// Assert
		expect(progressHub.start).toHaveBeenCalledTimes(1);
		expect(downloadHub.start).toHaveBeenCalledTimes(1);
		expect(notificationsHub.start).toHaveBeenCalledTimes(1);
	});

	test('Should not start already connected hub connections when setup is run', async () => {
		// Arrange
		hubConnections.set('progress', createHub(HubConnectionState.Connected));
		hubConnections.set('download', createHub(HubConnectionState.Connected));
		hubConnections.set('notifications', createHub(HubConnectionState.Connected));
		const signalrStore = useSignalrStore();
		const progressHub = hubConnections.get('progress')!;
		const downloadHub = hubConnections.get('download')!;
		const notificationsHub = hubConnections.get('notifications')!;

		// Act
		await subscribeSpyTo(signalrStore.setup()).onComplete();

		// Assert
		expect(progressHub.start).toHaveBeenCalledTimes(0);
		expect(downloadHub.start).toHaveBeenCalledTimes(0);
		expect(notificationsHub.start).toHaveBeenCalledTimes(0);
	});

	test('Should refresh library sync status once after the notification hub reconnects', async () => {
		// Arrange
		const signalrStore = useSignalrStore();
		const libraryStore = useLibraryStore();
		const notificationsHub = hubConnections.get('notifications')!;
		const queue = generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Queued });
		mock.onGet(PlexLibraryPaths.getLibrarySyncStatusEndpoint()).reply(200, generateResultDTO([queue]));

		// Act
		await subscribeSpyTo(signalrStore.setup()).onComplete();
		notificationsHub.reconnected?.();
		await flushPromises();

		// Assert
		expect(mock.history.get.filter((request) => request.url === PlexLibraryPaths.getLibrarySyncStatusEndpoint())).toHaveLength(1);
		expect(libraryStore.getLibrarySyncQueue(11)).toEqual(queue);
	});
});

function createHub(state = HubConnectionState.Disconnected): MockHub {
	const hub: MockHub = {
		state,
		handlers: new Map<string, (data: unknown) => void>(),
		on: vi.fn(),
		onreconnected: (callback) => {
			hub.reconnected = callback;
		},
		start: vi.fn(async () => Promise.resolve()),
	};

	return hub;
}
