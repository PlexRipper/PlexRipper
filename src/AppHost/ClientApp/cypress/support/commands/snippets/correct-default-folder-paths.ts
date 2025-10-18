import { FolderType } from '@dto';
import { kebabCase } from 'lodash-es';

Cypress.Commands.add('correctDefaultFolderPaths', () => {
	const folderTypes = [FolderType.DownloadFolder, FolderType.MovieFolder, FolderType.TvShowFolder];

	// NOTE: FolderPaths are already set in /cypress/fixtures/api-mock/folder-paths.mock-api.ts
	for (const folderType of folderTypes) {
		// Update DownloadFolder
		cy.getCy(`default-${kebabCase(folderType)}-valid-icon`).should('have.class', 'invalid-icon');
		cy.getCy(`default-${kebabCase(folderType)}-edit-button`).click();

		// Go to the root directory
		cy.getCy('directory-browser-row-return').click();
		const directory = folderType.replace('Folder', 's');
		cy.getCy('directory-browser-rows').within(() => cy.contains('tr', directory).click());

		cy.getCy('directory-browser-confirm-button').click();
		cy.getCy(`default-${kebabCase(folderType)}-valid-icon`).should('have.class', 'valid-icon');
		cy.getCy(`default-${kebabCase(folderType)}-input`).should('have.value', '/' + directory);
	}
});

export {};
