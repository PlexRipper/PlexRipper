import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { HubConnectionState } from '@microsoft/signalr';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { useSignalrStore } from '@store';

type HubName = 'progress' | 'download' | 'notifications';

interface MockHub {
	state: HubConnectionState;
	handlers: Map<string, (data: unknown) => void>;
	on: ReturnType<typeof vi.fn>;
	start: ReturnType<typeof vi.fn>;
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
});

function createHub(state = HubConnectionState.Disconnected) {
	return {
		state,
		handlers: new Map<string, (data: unknown) => void>(),
		on: vi.fn(),
		start: vi.fn(async () => Promise.resolve()),
	};
}
