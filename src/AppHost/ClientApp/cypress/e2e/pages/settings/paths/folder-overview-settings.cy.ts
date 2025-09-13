import { route } from '@fixtures';

describe('Change Folder Paths', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 0,
			plexServerCount: 0,
			invalidDefaultFolderPaths: true,
		});

		cy.visit(route('/settings/paths'));
	});

	it('Should set the correct destination for each default folder when using the directory browser', () => {
		cy.getPageData().then(() => cy.correctDefaultFolderPaths());
	});
});
