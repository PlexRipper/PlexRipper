import { beforeAll, beforeEach, describe, expect, test, vi, type Mock } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { HubConnectionState } from '@microsoft/signalr';
import { baseSetup, subscribeSpyTo } from '@services-test-base';
import { HubName, useSignalrStore } from '@store';
import { type ISetupResult, StoreNames } from '@interfaces';

interface MockHub {
	state: HubConnectionState;
	handlers: Map<string, (data: unknown) => void>;
	on: Mock<(event: string, callback: (data: unknown) => void) => void>;
	off: Mock<(event: string) => void>;
	start: Mock<() => Promise<void>>;
	stop: Mock<() => Promise<void>>;
}

const { hubConnections } = vi.hoisted(() => ({
	hubConnections: new Map<HubName, MockHub>(),
}));

vi.mock('cypress-signalr-mock', () => ({
	useCypressSignalRMock: (hubName: string) => hubConnections.get(hubName as HubName),
}));

describe('SignalrStore.setup() failure handling', () => {
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		setActivePinia(createPinia());
		hubConnections.clear();
		hubConnections.set(HubName.Progress, createHub());
		hubConnections.set(HubName.Download, createHub(new Error('Download hub failed')));
		hubConnections.set(HubName.Notifications, createHub());
		hubConnections.set(HubName.Logs, createHub());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		hubConnections.set(HubName.Download, createHub());
		const signalrStore = useSignalrStore();

		const setupResult: ISetupResult = {
			isSuccess: true,
			name: StoreNames.SignalrStore,
		};

		// Act
		const result = subscribeSpyTo(signalrStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});

	test('Should report failure when a hub connection fails to start', async () => {
		// Arrange
		const signalrStore = useSignalrStore();
		const setupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.SignalrStore,
		};

		// Act
		const result = subscribeSpyTo(signalrStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
		expect(result.receivedError()).toEqual(false);
		for (const hub of hubConnections.values()) {
			expect(hub.stop).toHaveBeenCalledOnce();
			expect(hub.handlers.size).toBe(0);
		}
	});
});

function createHub(startError?: Error, state = HubConnectionState.Disconnected): MockHub {
	const handlers = new Map<string, (data: unknown) => void>();

	return {
		state,
		handlers,
		on: vi.fn((event: string, callback: (data: unknown) => void) => {
			handlers.set(event, callback);
		}),
		off: vi.fn((event: string) => { handlers.delete(event); }),
		stop: vi.fn(async () => Promise.resolve()),
		start: vi.fn(async () => {
			if (startError) {
				throw startError;
			}

			return Promise.resolve();
		}),
	};
}
