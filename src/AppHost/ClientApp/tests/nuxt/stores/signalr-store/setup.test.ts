import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { HubConnectionState } from '@microsoft/signalr';
import { baseSetup, subscribeSpyTo } from '@services-test-base';
import { HubName, useSignalrStore } from '@store';
import { type ISetupResult, StoreNames } from '@interfaces';

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
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
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
		start: vi.fn(async () => {
			if (startError) {
				throw startError;
			}

			return Promise.resolve();
		}),
	};
}
