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
});
