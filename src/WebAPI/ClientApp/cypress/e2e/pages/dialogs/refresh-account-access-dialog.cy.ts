import { PlexAccountPaths } from '@api-urls';
import { generateResultDTO } from '@mock';
import { rand, randBoolean } from '@ngneat/falso';

import {
	PlexAccessState,
	type PlexLibraryAccessRapportDTO,
	type PlexServerAccessRapportDTO,
	type RefreshPlexAccountAccessRapportDTO,
} from '@dto';

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
			const data = plexAccounts.map((account): RefreshPlexAccountAccessRapportDTO => ({
				plexAccountId: account.id,
				plexAccountName: account.displayName,
				access: plexServers.map((server): PlexServerAccessRapportDTO => {
					const isServerOffline = randBoolean();
					return ({
						plexServerId: server.id,
						isServerOffline,
						plexServerName: server.name,
						state: isServerOffline ? PlexAccessState.Unknown : rand([PlexAccessState.Granted, PlexAccessState.Updated, PlexAccessState.Revoked]),
						libraryAccess: isServerOffline
							? []
							: plexLibraries.filter((library) => library.plexServerId == server.id).map((library): PlexLibraryAccessRapportDTO => ({
									plexLibraryName: library.title,
									plexLibraryId: library.id,
									plexServerId: library.plexServerId,
									state: rand([PlexAccessState.Granted, PlexAccessState.Updated, PlexAccessState.Revoked]),
								})),
					});
				}),
			}));

			cy.getCy('account-selector-btn').click();

			const plexAccountIds = plexAccounts.map((x) => x.id);
			for (const plexAccountId of [0, ...plexAccountIds]) {
				cy.intercept('GET', PlexAccountPaths.refreshPlexAccountAccessEndpoint(plexAccountId), {
					statusCode: 200,
					body: generateResultDTO(plexAccountId === 0 ? data : [data.find((x) => x.plexAccountId == plexAccountId)]),
				});

				cy.getCy(`refresh-account-${plexAccountId}-btn`).click();

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
