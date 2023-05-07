import { subscribeSpyTo, baseSetup, baseVars } from '@services-test-base';
import AlertService from '@service/alertService';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('AlertService.setup()', () => {
	baseVars();

	beforeAll(() => {
		baseSetup();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const setup$ = AlertService.setup();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: AlertService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).to.equal(true);
	});
});
