import { route } from '@fixtures';

describe('Home page', () => {
	it('Should redirect to the login page when not logged in and then to the home page when logged in', () => {
		cy.basePageSetup({
			plexServerCount: 1,
			plexAccountCount: 1,
			movieCount: 0,
		});
		cy.visit(route('/'));
		cy.url().should('eq', route('/'));

		cy.getPageData().then(() => {
			// Login
		});
	});
});
