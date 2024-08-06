import { MessageTypes } from '@dto';
import type { SyncServerProgress } from '@dto';
import { generateSyncServerProgress } from '@factories';

describe('Display the progress bar in the app bar', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 5,
		});

		cy.visitEmptyPage();
	});

	it('Should navigate the server dialog tabs when the navigation tabs are used and then close again', () => {
		cy.getPageData().then((data) => {
			for (let i = 0; i <= 10; i++) {
				const progress: SyncServerProgress[] = [];

				for (const plexServer of data.plexServers) {
					progress.push(generateSyncServerProgress({
						progressIndex: i,
						plexServerId: plexServer.id,
						plexLibraryIds: data.plexLibraries.filter((x) => x.plexServerId === plexServer.id).map((x) => x.id),
					}));
				}

				cy.wait(500).hubPublish('progress', MessageTypes.SyncServerProgress, progress);
				cy.log('progress', progress);
			}
		});
	});
});
