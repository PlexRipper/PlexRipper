import { DialogType } from '@enums';
import { PlexAccountPaths } from '@api-urls';
import { generateRefreshPlexAccountAccessRapportDTO, generateResultDTO } from '@mock';

describe('Side bar', () => {
	it('Should display no library warning and allow the user to refresh the Plex account access when a server has no libraries', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 3,
			plexMovieLibraryCount: 0,
			plexTvShowLibraryCount: 0,
		});
		cy.visitEmptyPage();

		cy.getPageData().then(({ plexAccounts, plexServers, plexLibraries }) => {
			const data = generateRefreshPlexAccountAccessRapportDTO({
				plexAccounts, plexLibraries, plexServers,
			});

			cy.intercept('GET', PlexAccountPaths.refreshPlexAccountAccessEndpoint(0), {
				statusCode: 200,
				body: generateResultDTO(data), delay: 200,
			});

			for (const server of plexServers) {
				cy.getCy(`server-drawer-item-${server.id}`).click();
				cy.getCy(`server-drawer-item-${server.id}-no-libraries`).click();
				cy.getCy(`server-drawer-item-${server.id}-refresh-loading`).should('be.visible');

				cy.getCy(DialogType.RefreshAccountAccessDialog).should('be.visible');
				cy.getCy('refresh-account-access-dialog-hide-btn').click();
				cy.getCy(`server-drawer-item-${server.id}-refresh-loading`).should('not.exist');
			}
		});
	});
});
