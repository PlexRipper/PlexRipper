import { FolderType } from '@dto';
import { FolderPathPaths } from '@api/api-paths';
import { kebabCase } from 'lodash-es';

Cypress.Commands.add('correctDefaultFolderPaths', () => {
	const folderTypes = [FolderType.DownloadFolder, FolderType.MovieFolder, FolderType.TvShowFolder];
	cy.intercept('PUT', FolderPathPaths.updateFolderPathEndpoint()).as('updateFolderPath');

	// NOTE: FolderPaths are already set in /cypress/fixtures/api-mock/folder-paths.mock-api.ts
	for (const folderType of folderTypes) {
		const tabSelector = `[data-cy="folder-path-tab-${kebabCase(folderType)}"]`;
		cy.get('body').then(($body) => {
			if ($body.find(tabSelector).length > 0) {
				cy.getCy(`folder-path-tab-${kebabCase(folderType)}`).click();
			}
		});

		// Update DownloadFolder
		cy.getCy(`default-${kebabCase(folderType)}-valid-icon`).should('have.class', 'invalid-icon');
		cy.getCy(`default-${kebabCase(folderType)}-edit-button`).click();

		// Go to the root directory
		cy.getCy('directory-browser-row-return').click();
		const directory = folderType.replace('Folder', 's');
		cy.getCy('directory-browser-rows').within(() => cy.contains('tr', directory).click());

		cy.getCy('directory-browser-confirm-button').click();
		cy.wait('@updateFolderPath').then(({ request, response }) => {
			expect(request.body.directory).to.equal('/' + directory);
			expect(response?.body.value).to.deep.include({
				id: request.body.id,
				directory: '/' + directory,
				isValid: true,
			});
		});
		cy.getCy(`default-${kebabCase(folderType)}-valid-icon`).should('have.class', 'valid-icon');
		cy.getCy(`default-${kebabCase(folderType)}-input`).should('have.value', '/' + directory);
	}
});

export {};
