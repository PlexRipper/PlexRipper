import { PlexAccountPaths } from '@api-urls';
import { generateResultDTO } from '@mock';
import { rand } from '@ngneat/falso';

import {	PlexAccessState,	type PlexLibraryAccessRapportDTO,	type PlexServerAccessRapportDTO } from '@dto';

describe('Refresh Account Access Dialog', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 5,
			plexServerCount: 5,
			plexTvShowLibraryCount: 5,
			plexMovieLibraryCount: 5,
		});

		cy.visitEmptyPage();
	});

	it('Should navigate the server dialog tabs when the navigation tabs are used and then close again', () => {
		cy.getPageData().then(({ plexAccounts, plexServers, plexLibraries }) => {
			const data = plexAccounts.map((account) => ({
				plexAccountId: account.id,
				access: plexServers.map((server): PlexServerAccessRapportDTO => ({
					plexServerId: server.id,
					state: rand([PlexAccessState.Granted, PlexAccessState.Updated, PlexAccessState.Revoked]),
					libraryAccess: plexLibraries.filter((library) => library.plexServerId == server.id).map((library): PlexLibraryAccessRapportDTO => ({
						plexLibraryId: library.id,
						plexServerId: library.plexServerId,
						state: rand([PlexAccessState.Granted, PlexAccessState.Updated, PlexAccessState.Revoked]),
					})),
				})),
			}));

			cy.getCy('account-selector-btn').click();

			const plexAccountIds = plexAccounts.map((x) => x.id);
			for (const plexAccountId of [0, ...plexAccountIds]) {
				cy.intercept('GET', PlexAccountPaths.refreshPlexAccountAccessEndpoint(plexAccountId), {
					statusCode: 200,
					body: generateResultDTO(plexAccountId === 0 ? data : [data.find((x) => x.plexAccountId == plexAccountId)]),
				});

				cy.getCy(`refresh-account-${plexAccountId}-btn`).click();

				if (plexAccountId === 0) {
					for (const id of plexAccountIds) {
						cy.getCy(`refresh-account-access-tab-${id}`).click();
					}
				} else {
					cy.getCy(`refresh-account-access-tab-${plexAccountId}`).click();
				}

				cy.getCy('refresh-account-access-dialog-hide-btn').click();
			}
		});
	});
});
