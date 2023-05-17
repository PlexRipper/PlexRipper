// @vitest-environment node
// We need node due to the use of Object.createURL()

import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import MediaService from '@service/mediaService';
import { PlexMediaType } from '@dto/mainApi';
import { PLEX_MEDIA_RELATIVE_PATH } from '@api-urls';

describe('MediaService.getThumbnail()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
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
		expect(getThumbnailResult.receivedComplete()).to.equal(true);
		expect(getThumbnailResult.getFirstValue()).toBeTruthy();
	});
});
