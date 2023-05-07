import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import FolderPathService from '@service/folderPathService';
import { generateResultDTO } from '@mock';
import { FOLDER_PATH_RELATIVE_PATH } from '@api-urls';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('FolderPathService.setup()', () => {
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
		mock.onGet(FOLDER_PATH_RELATIVE_PATH).reply(200, generateResultDTO([]));
		const setup$ = FolderPathService.setup();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: FolderPathService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
