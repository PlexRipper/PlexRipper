import { PlexAccountPaths } from '@api-urls';
import { generateRefreshPlexAccountAccessRapportDTO, generateResultDTO } from '@mock';

import { DialogType } from '@enums';

describe('Refresh Account Access Dialog', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 3,
			plexServerCount: 5,
			plexTvShowLibraryCount: 5,
			plexMovieLibraryCount: 5,
		});

		cy.visitEmptyPage();
	});

	it('Should navigate the plex account refresh dialog tabs when multiple account access are refreshed', () => {
		cy.getPageData().then(({ plexAccounts, plexServers, plexLibraries }) => {
			const data = generateRefreshPlexAccountAccessRapportDTO({
				plexAccounts, plexLibraries, plexServers,
			});

			cy.getCy('account-selector-btn').click();

			const plexAccountIds = plexAccounts.map((x) => x.id);
			for (const plexAccountId of [0, ...plexAccountIds]) {
				cy.intercept('GET', PlexAccountPaths.refreshPlexAccountAccessEndpoint(plexAccountId), {
					statusCode: 200,
					body: generateResultDTO(plexAccountId === 0 ? data : [data.find((x) => x.plexAccountId == plexAccountId)]),
				});

				cy.getCy(`refresh-account-${plexAccountId}-btn`).click();

				cy.getCy(DialogType.RefreshAccountAccessDialog).should('be.visible');

				function verifyTab(plexAccountId: number) {
					const rapport = data.find((x) => x.plexAccountId == plexAccountId)!;

					cy.getCy(`refresh-account-access-tree-${plexAccountId}`).within(() => {
						for (const server of rapport.access) {
							// Server row
							cy.getCy(`access-row-server-${server.plexServerId}`).within(() => {
								// Server Title
								cy.getCy(`access-dialog-title-server-${server.plexServerId}`)
									.should('contain', server.plexServerName);

								if (server.isServerOffline) {
									cy.getCy('status-indicator-offline').should('be.visible');
								}
							});

							// Server offline message
							if (server.isServerOffline) {
								cy.getCy(`access-dialog-server-offline-text-${server.plexServerId}`).should('exist');
							}

							for (const library of server.libraryAccess) {
								cy.getCy(`access-row-server-${server.plexServerId}-lib-${library.plexLibraryId}`).within(() => {
									// Server Title
									cy.getCy(`access-dialog-title-server-${server.plexServerId}-lib-${library.plexLibraryId}`)
										.should('contain', library.plexLibraryName);
								});
							}
						}
					});
				}

				if (plexAccountId === 0) {
					for (const id of plexAccountIds) {
						cy.getCy(`refresh-account-access-tab-${id}`).click();
						verifyTab(id);
					}
				} else {
					cy.getCy(`refresh-account-access-tab-${plexAccountId}`).click();
					verifyTab(plexAccountId);
				}

				cy.getCy('refresh-account-access-dialog-hide-btn').click();
			}
		});
	});
});
