import { describe, beforeAll, beforeEach, test, expect, assert } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { Subject, takeUntil } from 'rxjs';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { UpdatePaths } from '@api-urls';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { useUpdateStore } from '@store';

describe('UpdateStore.downloadAndApplyUpdate()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
		mock.onGet(UpdatePaths.checkForUpdateEndpoint()).reply(200, generateResultDTO({
			currentVersion: '1.0.0', newestVersion: '1.0.1', isUpdateAvailable: true, releaseNotes: [],
		}));
	});

	test.each([200, 500, 'network'] as const)('Should preserve a failed download and allow a corrected retry for %s', async (failureMode) => {
		// Arrange
		const store = useUpdateStore();
		await subscribeSpyTo(store.setup()).onComplete();
		const failure = generateFailedResultDTO({
			statusCode: failureMode === 'network' ? 999 : failureMode,
			errors: [{ message: 'Update package download failed', metadata: {}, reasons: [] }],
		});
		if (failureMode === 'network') {
			mock.onPost(UpdatePaths.downloadUpdateEndpoint()).networkErrorOnce();
		} else {
			mock.onPost(UpdatePaths.downloadUpdateEndpoint()).replyOnce(failureMode, failure);
		}
		mock.onPost(UpdatePaths.downloadUpdateEndpoint()).reply(200, generateResultDTO(null));
		mock.onPost(UpdatePaths.applyUpdateEndpoint()).reply(200, generateResultDTO(null));

		// Act
		const failed = subscribeSpyTo(store.downloadAndApplyUpdate());
		await failed.onComplete();

		// Assert
		expect(failed.getValues()).toHaveLength(1);
		const failedResult = failed.getLastValue();
		assert.isDefined(failedResult);
		expect(failedResult.isSuccess).toBe(false);
		expect(failedResult.statusCode).toBe(failureMode === 'network' ? 999 : failureMode);
		expect(failedResult.errors).toEqual(failureMode === 'network'
			? [{ message: 'Internal Server Error', reasons: [], metadata: {} }]
			: failure.errors);
		expect(mock.history.post.map((request) => request.url)).toEqual([UpdatePaths.downloadUpdateEndpoint()]);
		expect(store.isDownloading).toBe(false);
		expect(store.isApplyingUpdate).toBe(false);

		// Act
		const retry = subscribeSpyTo(store.downloadAndApplyUpdate());
		await retry.onComplete();

		// Assert
		const retryResult = retry.getLastValue();
		assert.isDefined(retryResult);
		expect(retryResult.isSuccess).toBe(true);
		expect(retryResult.errors).toEqual([]);
		expect(mock.history.post.map((request) => request.url)).toEqual([
			UpdatePaths.downloadUpdateEndpoint(), UpdatePaths.downloadUpdateEndpoint(), UpdatePaths.applyUpdateEndpoint(),
		]);
		expect(store.isDownloading).toBe(false);
		expect(store.isApplyingUpdate).toBe(false);
	});

	test.each([false, true])('Should enter apply only after download succeeds and finalize its result when apply fails=%s', async (applyFails) => {
		// Arrange
		const store = useUpdateStore();
		await subscribeSpyTo(store.setup()).onComplete();
		const download = Promise.withResolvers<[number, unknown]>();
		const apply = Promise.withResolvers<[number, unknown]>();
		const enteredApply = Promise.withResolvers<undefined>();
		const applyResult = applyFails
			? generateFailedResultDTO({ statusCode: 500, errors: [{ message: 'Apply failed', metadata: {}, reasons: [] }] })
			: generateResultDTO(null);
		mock.onPost(UpdatePaths.downloadUpdateEndpoint()).reply(() => download.promise);
		mock.onPost(UpdatePaths.applyUpdateEndpoint()).reply(() => {
			enteredApply.resolve(undefined);
			return apply.promise;
		});

		// Act
		const result = subscribeSpyTo(store.downloadAndApplyUpdate());
		expect(store.isDownloading).toBe(true);
		expect(store.isApplyingUpdate).toBe(false);
		download.resolve([200, generateResultDTO(null)]);
		await enteredApply.promise;

		// Assert
		expect(store.isDownloading).toBe(true);
		expect(store.isApplyingUpdate).toBe(true);
		expect(mock.history.post.map((request) => request.url)).toEqual([
			UpdatePaths.downloadUpdateEndpoint(), UpdatePaths.applyUpdateEndpoint(),
		]);
		apply.resolve([applyFails ? 500 : 200, applyResult]);
		await result.onComplete();
		const completedResult = result.getLastValue();
		assert.isDefined(completedResult);
		expect(completedResult.isSuccess).toBe(!applyFails);
		expect(completedResult.errors).toEqual(applyResult.errors);
		expect(store.isDownloading).toBe(false);
		expect(store.isApplyingUpdate).toBe(false);
	});

	test('Should clear busy state without applying when the caller cancels during download', async () => {
		// Arrange
		const store = useUpdateStore();
		await subscribeSpyTo(store.setup()).onComplete();
		const download = Promise.withResolvers<[number, unknown]>();
		const cancelled = new Subject<void>();
		mock.onPost(UpdatePaths.downloadUpdateEndpoint()).reply(() => download.promise);

		// Act
		const result = subscribeSpyTo(store.downloadAndApplyUpdate().pipe(takeUntil(cancelled)));
		expect(store.isDownloading).toBe(true);
		cancelled.next();
		await result.onComplete();
		download.resolve([200, generateResultDTO(null)]);

		// Assert
		expect(result.getValues()).toEqual([]);
		expect(store.isDownloading).toBe(false);
		expect(store.isApplyingUpdate).toBe(false);
		expect(mock.history.post.filter((request) => request.url === UpdatePaths.applyUpdateEndpoint())).toEqual([]);
		cancelled.complete();
	});
});
