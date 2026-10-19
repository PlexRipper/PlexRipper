import { route } from '@fixtures';
import { UpdatePaths } from '@api-urls';
import { generateFailedResultDTO, generateResultDTO } from '@mock';

describe('Desktop update download failure', () => {
	it('Should preserve the download error without applying, then allow a corrected retry', () => {
		cy.basePageSetup({ isLoggedIn: true, plexAccountCount: 0, plexServerCount: 0 }).then(() => {
			cy.intercept({ url: '**/api/**', middleware: true }, (request) => {
				request.on('before:response', (response) => {
					response.headers['x-reaparr-platform'] = 'desktop';
				});
			});
			cy.intercept('GET', UpdatePaths.checkForUpdateEndpoint(), {
				statusCode: 200,
				body: generateResultDTO({
					currentVersion: '1.0.0', newestVersion: '1.0.1', isUpdateAvailable: true,
					releaseNotes: [{ version: '1.0.1', releaseDate: '2026-10-18T00:00:00Z', notes: 'Download retry regression', isDevRelease: false }],
				}),
			}).as('checkUpdate');
			let downloadAttempts = 0;
			let applyCalls = 0;
			cy.intercept('POST', UpdatePaths.downloadUpdateEndpoint(), (request) => {
				downloadAttempts++;
				request.reply({
					statusCode: downloadAttempts === 1 ? 500 : 200,
					body: downloadAttempts === 1
						? generateFailedResultDTO({ statusCode: 500, errors: [{ message: 'Update package download failed', metadata: {}, reasons: [] }] })
						: generateResultDTO(null),
				});
			}).as('downloadUpdate');
			cy.intercept('POST', UpdatePaths.applyUpdateEndpoint(), (request) => {
				applyCalls++;
				request.reply({ statusCode: 200, body: generateResultDTO(null) });
			}).as('applyUpdate');

			cy.visit(route('/settings/ui'));
			cy.wait('@checkUpdate');
			cy.get('.app-bar .update-button').filter(':visible').click();
			cy.getCy('updatedialog-cy').should('contain.text', '1.0.1');
			cy.getCy('updatedialog-cy').contains('button', 'Download').should('not.be.disabled').click();
			cy.wait('@downloadUpdate').its('response.statusCode').should('equal', 500);
			cy.getCy('alert-dialog').should('contain.text', 'Update package download failed');
			cy.getCy('api-error').should('have.length', 1).and('contain.text', '/api/Update/DownloadUpdate');
			cy.then(() => expect(applyCalls).to.equal(0));
			cy.getCy('close-alert-dialog').click();
			cy.getCy('alert-dialog').should('not.exist');
			cy.getCy('updatedialog-cy').contains('button', 'Download').should('not.be.disabled').click();
			cy.wait('@downloadUpdate').its('response.statusCode').should('equal', 200);
			cy.wait('@applyUpdate').its('response.statusCode').should('equal', 200);
			cy.getCy('updatedialog-cy').contains('button', 'Download').should('not.be.disabled');
			cy.getCy('alert-dialog').should('not.exist');
			cy.then(() => {
				expect(downloadAttempts).to.equal(2);
				expect(applyCalls).to.equal(1);
			});
		});
	});
});
