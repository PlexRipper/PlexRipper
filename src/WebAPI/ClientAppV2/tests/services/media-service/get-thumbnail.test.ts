// @vitest-environment node
// We need node due to the use of Object.createURL()

import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import MediaService from '@service/mediaService';
import { PlexMediaType } from '@dto/mainApi';
import { PLEX_MEDIA_RELATIVE_PATH } from '@api-urls';

describe('MediaService.getThumbnail()', () => {
	let { ctx, mock } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const setup$ = MediaService.setup();
		const getThumbnail$ = MediaService.getThumbnail(1, PlexMediaType.Movie, 100, 100);
		const blob = new Blob([], { type: 'image/jpeg' });
		mock.onGet(`${PLEX_MEDIA_RELATIVE_PATH}/thumb`).reply(200, blob);

		// Act
		const setupResult = subscribeSpyTo(setup$);
		await setupResult.onComplete();
		const getThumbnailResult = subscribeSpyTo(getThumbnail$);
		await getThumbnailResult.onComplete();

		// Assert
		expect(getThumbnailResult.receivedComplete()).toEqual(true);
		expect(getThumbnailResult.getFirstValue()).not.toBeFalsy();
	});
});
