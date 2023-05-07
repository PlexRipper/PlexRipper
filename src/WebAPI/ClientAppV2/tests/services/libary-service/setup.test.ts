import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import LibraryService from '@service/libraryService';
import { generateResultDTO } from '@mock';
import { PLEX_LIBRARY_RELATIVE_PATH } from '@api-urls';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('LibraryService.setup()', () => {
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
		mock.onGet(PLEX_LIBRARY_RELATIVE_PATH).reply(200, generateResultDTO([]));
		const setup$ = LibraryService.setup(ctx);
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: LibraryService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
