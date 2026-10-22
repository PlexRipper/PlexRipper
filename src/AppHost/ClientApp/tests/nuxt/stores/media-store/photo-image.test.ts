import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { catchError, of } from 'rxjs';
import Axios from 'axios';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PhotoAssetKind, PhotoImageRepresentation, type PhotoAssetBrowseDTO } from '@dto';
import { useMediaStore } from '@store';

const asset: PhotoAssetBrowseDTO = {
	id: 501, plexServerId: 3, plexLibraryId: 41,
	plexServerName: 'Photo server', plexLibraryTitle: 'Photos', title: 'Original image',
	assetKind: PhotoAssetKind.Image, hasThumbnail: true,
	plexApiRatingKey: 501, plexApiMetaDataKey: 502,
	originalFilename: 'original.jpg', container: 'jpg', size: 4096, width: 6000, height: 4000,
};
const imagePath = '/api/PlexPhoto/libraries/41/assets/501/image';
function photoAsset(overrides: Partial<PhotoAssetBrowseDTO> = {}): PhotoAssetBrowseDTO {
	return { ...asset, ...overrides };
}

function blobWithSize(size: number): Blob {
	const image = new Blob(['image'], { type: 'image/jpeg' });
	Object.defineProperty(image, 'size', { configurable: true, value: size });
	return image;
}

describe('MediaStore photo image ownership', () => {
	let { mock } = baseVars();

	beforeAll(() => baseSetup());
	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});
	afterEach(() => vi.restoreAllMocks());

	test('Should fetch original image bytes even when the same asset has a cached thumbnail', async () => {
		// Arrange
		const createUrl = vi.spyOn(URL, 'createObjectURL')
			.mockReturnValueOnce('blob:thumbnail').mockReturnValueOnce('blob:original');
		const original = new Blob(['full-resolution-image'], { type: 'image/jpeg' });
		mock.onGet(imagePath, { params: { representation: PhotoImageRepresentation.Original } }).reply(200, original);
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();
		store.updateMediaUrl({ plexServerId: 3, plexKey: '501', metaDataKey: 502, image: new Blob(['thumbnail'], { type: 'image/jpeg' }) });

		// Act
		const response = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
		await response.onComplete();

		// Assert
		expect(response.getLastValue()).toBe('blob:original');
		expect(createUrl.mock.calls[1]?.[0]).toBe(original);
		expect(store.mediaUrls[0]?.url).toBe('blob:thumbnail');
	});

	test('Should release owned originals exactly once without revoking thumbnail URLs', async () => {
		// Arrange
		vi.spyOn(URL, 'createObjectURL').mockReturnValueOnce('blob:original').mockReturnValueOnce('blob:converted');
		const revoke = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => undefined);
		mock.onGet(imagePath).reply(200, new Blob(['image'], { type: 'image/png' }));
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();
		const original = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
		await original.onComplete();
		const converted = subscribeSpyTo(store.getPhotoImageUrl(asset, 'converted'));
		await converted.onComplete();

		// Act
		store.releasePhotoImageUrl(original.getLastValue()!);
		store.releasePhotoImageUrl(original.getLastValue()!);
		store.releasePhotoImageUrl('blob:thumbnail');
		store.$reset();
		store.releasePhotoImageUrl(converted.getLastValue()!);

		// Assert
		expect(revoke.mock.calls).toEqual([['blob:original'], ['blob:converted']]);
	});

	test('Should reject a successful non-image payload instead of presenting an error blob as an image', async () => {
		// Arrange
		const createUrl = vi.spyOn(URL, 'createObjectURL');
		mock.onGet(imagePath).reply(200, new Blob(['{"isSuccess":false}'], { type: 'application/json' }));
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		const response = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
		await response.onComplete();

		// Assert
		expect(response.getLastValue()).toBe('');
		expect(createUrl).not.toHaveBeenCalled();
	});

	test('Should preserve missing catalog identity errors instead of treating them as a preview failure', async () => {
		// Arrange
		mock.onGet(imagePath).reply(404, { isSuccess: false, errors: [], value: null });
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		const response = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original').pipe(catchError((error: unknown) => of(error))));
		await response.onComplete();

		// Assert
		expect(response.getLastValue()).toBeInstanceOf(Error);
		expect(response.getLastValue()).toMatchObject({ statusCode: 404 });
	});

	test('Should keep an unavailable Plex image in the viewer error path rather than the global request-error pipeline', async () => {
		// Arrange
		mock.onGet(imagePath).reply(502, new Blob(['Unavailable'], { type: 'application/json' }));
		const rejected = vi.fn((error: unknown) => Promise.reject(error));
		const interceptor = Axios.interceptors.response.use(undefined, rejected);
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		try {
			// Act
			const response = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
			await response.onComplete();

			// Assert
			expect(response.getLastValue()).toBe('');
			expect(rejected).not.toHaveBeenCalled();
		} finally {
			Axios.interceptors.response.eject(interceptor);
		}
	});
	test('Should reuse a preloaded original for fullscreen without another network request', async () => {
		// Arrange
		vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:original');
		const original = new Blob(['full-resolution-image'], { type: 'image/jpeg' });
		mock.onGet(imagePath).reply(200, original);
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		const preload = subscribeSpyTo(store.preloadPhotoImage(asset));
		await preload.onComplete();
		const fullscreen = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
		await fullscreen.onComplete();

		// Assert
		expect(fullscreen.getLastValue()).toBe('blob:original');
		expect(mock.history.get.filter((request) => request.url === imagePath)).toHaveLength(1);
		store.releasePhotoImageUrl(fullscreen.getLastValue()!);
	});

	test('Should join a pending preload from a foreground request', async () => {
		// Arrange
		vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:original');
		const original = new Blob(['full-resolution-image'], { type: 'image/jpeg' });
		let resolvePending!: (response: [number, Blob]) => void;
		const pending = new Promise<[number, Blob]>((resolve) => {
			resolvePending = resolve;
		});
		mock.onGet(imagePath).replyOnce(() => pending);
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		const preload = subscribeSpyTo(store.preloadPhotoImage(asset));
		const fullscreen = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
		resolvePending([200, original]);
		await Promise.all([preload.onComplete(), fullscreen.onComplete()]);

		// Assert
		expect(fullscreen.getLastValue()).toBe('blob:original');
		expect(mock.history.get.filter((request) => request.url === imagePath)).toHaveLength(1);
		store.releasePhotoImageUrl(fullscreen.getLastValue()!);
	});

	test('Should separate original, converted, and library image cache identities', async () => {
		// Arrange
		const createUrl = vi.spyOn(URL, 'createObjectURL')
			.mockReturnValueOnce('blob:original')
			.mockReturnValueOnce('blob:converted')
			.mockReturnValueOnce('blob:other-library');
		mock.onGet(new RegExp('/api/PlexPhoto/libraries/\\d+/assets/\\d+/image')).reply(200, new Blob(['image'], { type: 'image/jpeg' }));
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();
		const otherLibraryAsset = photoAsset({ id: 601, plexLibraryId: 42 });

		// Act
		const original = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
		await original.onComplete();
		const converted = subscribeSpyTo(store.getPhotoImageUrl(asset, 'converted'));
		await converted.onComplete();
		const otherLibrary = subscribeSpyTo(store.getPhotoImageUrl(otherLibraryAsset, 'original'));
		await otherLibrary.onComplete();

		// Assert
		expect(original.getLastValue()).toBe('blob:original');
		expect(converted.getLastValue()).toBe('blob:converted');
		expect(otherLibrary.getLastValue()).toBe('blob:other-library');
		expect(mock.history.get.filter((request) => request.url?.includes('/image'))).toHaveLength(3);
		expect(createUrl).toHaveBeenCalledTimes(3);
		store.releasePhotoImageUrl(original.getLastValue()!);
		store.releasePhotoImageUrl(converted.getLastValue()!);
		store.releasePhotoImageUrl(otherLibrary.getLastValue()!);
	});

	test('Should abort an in-flight request when its last consumer unsubscribes', async () => {
		// Arrange
		const abort = vi.spyOn(AbortController.prototype, 'abort');
		let resolvePending!: (response: [number, Blob]) => void;
		const pending = new Promise<[number, Blob]>((resolve) => {
			resolvePending = resolve;
		});
		mock.onGet(imagePath).replyOnce(() => pending);
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		const preload = subscribeSpyTo(store.preloadPhotoImage(asset));
		preload.unsubscribe();
		resolvePending([200, new Blob(['stale'], { type: 'image/jpeg' })]);

		// Assert
		expect(abort).toHaveBeenCalled();
	});

	test('Should invalidate pending and cached images without allowing stale refill', async () => {
		// Arrange
		const createUrl = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:fresh');
		let resolvePending!: (response: [number, Blob]) => void;
		const pending = new Promise<[number, Blob]>((resolve) => {
			resolvePending = resolve;
		});
		mock.onGet(imagePath).replyOnce(() => pending).onGet(imagePath).reply(200, new Blob(['fresh'], { type: 'image/jpeg' }));
		const revoke = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => undefined);
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		const preload = subscribeSpyTo(store.preloadPhotoImage(asset));
		store.invalidatePhotoImages(new Set([asset.plexLibraryId]));
		resolvePending([200, new Blob(['stale'], { type: 'image/jpeg' })]);
		await preload.onComplete();
		const fresh = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
		await fresh.onComplete();
		store.invalidatePhotoImages(new Set([asset.plexLibraryId]));

		// Assert
		expect(createUrl).toHaveBeenCalledTimes(1);
		expect(fresh.getLastValue()).toBe('blob:fresh');
		expect(revoke).not.toHaveBeenCalledWith('blob:fresh');
		store.releasePhotoImageUrl(fresh.getLastValue()!);
		expect(revoke).toHaveBeenCalledWith('blob:fresh');
	});

	test('Should evict least-recently-used unleased images at the count limit', async () => {
		// Arrange
		let imageIndex = 0;
		const createUrl = vi.spyOn(URL, 'createObjectURL')
			.mockImplementation(() => `blob:image-${++imageIndex}`);
		const revoke = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => undefined);
		mock.onGet(new RegExp('/api/PlexPhoto/libraries/\\d+/assets/\\d+/image')).reply(200, new Blob(['image'], { type: 'image/jpeg' }));
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		for (let id = 1; id <= 25; id += 1) {
			const preload = subscribeSpyTo(store.preloadPhotoImage(photoAsset({ id })));
			await preload.onComplete();
		}

		// Assert
		expect(createUrl).toHaveBeenCalledTimes(25);
		expect(revoke).toHaveBeenCalledWith('blob:image-1');
	});

	test('Should keep leased viewer images valid while evicting oversized unleased images', async () => {
		// Arrange
		vi.spyOn(URL, 'createObjectURL')
			.mockReturnValueOnce('blob:pinned')
			.mockReturnValueOnce('blob:oversized');
		const revoke = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => undefined);
		const oversized = blobWithSize(64 * 1024 * 1024 + 1);
		mock.onGet(new RegExp('/api/PlexPhoto/libraries/\\d+/assets/\\d+/image')).reply(200, oversized);
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		const pinned = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
		await pinned.onComplete();
		const preload = subscribeSpyTo(store.preloadPhotoImage(photoAsset({ id: 602 })));
		await preload.onComplete();

		// Assert
		expect(revoke).toHaveBeenCalledWith('blob:oversized');
		expect(revoke).not.toHaveBeenCalledWith('blob:pinned');
		store.releasePhotoImageUrl(pinned.getLastValue()!);
	});

	test('Should retry a failed image request instead of reusing a poisoned observable', async () => {
		// Arrange
		vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:retry');
		mock.onGet(imagePath)
			.replyOnce(404, { isSuccess: false, errors: [], value: null })
			.onGet(imagePath)
			.reply(200, new Blob(['retry'], { type: 'image/jpeg' }));
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		const failed = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original').pipe(catchError((error: unknown) => of(error))));
		await failed.onComplete();
		const retried = subscribeSpyTo(store.getPhotoImageUrl(asset, 'original'));
		await retried.onComplete();

		// Assert
		expect(failed.getLastValue()).toMatchObject({ statusCode: 404 });
		expect(retried.getLastValue()).toBe('blob:retry');
		expect(mock.history.get.filter((request) => request.url === imagePath)).toHaveLength(2);
		store.releasePhotoImageUrl(retried.getLastValue()!);
	});

	test('Should skip a known oversized preload but retain a foreground image lease until release', async () => {
		// Arrange
		const largeAsset = photoAsset({ size: 64 * 1024 * 1024 + 1 });
		const createUrl = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:large-original');
		const revoke = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => undefined);
		mock.onGet(imagePath).reply(200, blobWithSize(largeAsset.size));
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		await subscribeSpyTo(store.preloadPhotoImage(largeAsset)).onComplete();

		// Assert
		expect(mock.history.get.filter((request) => request.url === imagePath)).toHaveLength(0);
		expect(createUrl).not.toHaveBeenCalled();
		const foreground = subscribeSpyTo(store.getPhotoImageUrl(largeAsset, 'original'));
		await foreground.onComplete();
		expect(foreground.getLastValue()).toBe('blob:large-original');
		expect(mock.history.get.filter((request) => request.url === imagePath)).toHaveLength(1);
		store.invalidatePhotoImages(new Set([largeAsset.plexLibraryId]));
		expect(revoke).not.toHaveBeenCalled();
		store.releasePhotoImageUrl(foreground.getLastValue()!);
		expect(revoke.mock.calls).toEqual([['blob:large-original']]);
		store.$reset();
	});

	test.each([0, 64 * 1024 * 1024])('Should reuse a preload when the catalog size is %s bytes', async (size) => {
		// Arrange
		const candidate = photoAsset({ size });
		vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:retained-original');
		mock.onGet(imagePath).reply(200, blobWithSize(size || 1));
		const store = useMediaStore();
		await subscribeSpyTo(store.setup()).onComplete();

		// Act
		await subscribeSpyTo(store.preloadPhotoImage(candidate)).onComplete();
		const foreground = subscribeSpyTo(store.getPhotoImageUrl(candidate, 'original'));
		await foreground.onComplete();

		// Assert
		expect(foreground.getLastValue()).toBe('blob:retained-original');
		expect(mock.history.get.filter((request) => request.url === imagePath)).toHaveLength(1);
		store.releasePhotoImageUrl(foreground.getLastValue()!);
		store.$reset();
	});
});
