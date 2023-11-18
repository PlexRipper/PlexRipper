import { APIRoute, apiRoute } from '@fixtures/baseE2E';
import { generateResultDTO } from '@mock';
import { SettingsModelDTO } from '@dto/mainApi';
describe('PlexRipper Server Dialog', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 5,
			plexLibraryCount: 5,
		});

		cy.visitEmptyPage();
	});

	xit('Should navigate the server dialog tabs when the navigation tabs are used and then close again', () => {
		cy.getCy('server-dialog-2').click();

		for (let i = 1; i <= 5; i++) {
			cy.getCy('server-dialog-tab-' + i).click();
			cy.getCy('server-dialog-tab-content-' + i)
				.should('exist')
				.and('be.visible');
		}

		cy.getCy('server-dialog-close-btn').click();
		cy.getCy('server-dialog-cy').should('not.exist');
	});

	xit('Should slide the download speed limit when the slider is used', () => {
		cy.getPageData().then((pageData) => {
			const plexServer = pageData.plexServers[2];
			cy.getCy('server-dialog-2').click();

			cy.getCy('server-dialog-tab-3').click();
			cy.getCy('server-dialog-tab-content-3').should('exist').and('be.visible');

			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');

			cy.getCy('download-speed-limit-slider').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const newSettings = interception.request.body as SettingsModelDTO;

				const serverSettings = newSettings.serverSettings.data.find(
					(x) => x.machineIdentifier === plexServer.machineIdentifier,
				);

				expect(serverSettings?.downloadSpeedLimit).to.eq(50000);
			});
		});
	});

	it('Should display the preferred server connection when it has been selected', () => {
		cy.getCy('server-dialog-3').click();
	});
});
