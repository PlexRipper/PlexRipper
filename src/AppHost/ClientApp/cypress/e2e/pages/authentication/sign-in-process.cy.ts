import { route } from '@fixtures';
import { AuthenticationPaths } from '@api-urls';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import type { AppUserLoginEndpointRequest } from '@dto';

describe('sign-in-process', () => {
	it('Should redirect to the login page when not logged in and then to the home page when logged in', () => {
		cy.basePageSetup({
			plexServerCount: 1,
			plexAccountCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 100,
			isLoggedIn: false,
		});

		cy.visit(route('/'));
		cy.url().should('eq', route('/login'));

		cy.getPageData().then(() => {
			// Login
			cy.intercept('POST', AuthenticationPaths.appUserLoginEndpoint(), {
				statusCode: 200,
				body: generateResultDTO<AppUserLoginEndpointRequest>({
					username: 'admin',
					password: 'password',
					rememberMe: false,
				}),
			}).as('loginSuccess');
			cy.interceptAuthenticationStatus(true);

			// Type credentials
			cy.getCy('login-username-input').type('admin');
			cy.getCy('login-password-input').type('password');
			cy.getCy('login-remember-me-input').click();
			cy.getCy('login-submit-button').click();
			cy.wait('@loginSuccess');

			cy.url().should('eq', route('/'));
		});
	});

	it('Should redirect to the login page when not logged in and then show invalid credentials when wrong', () => {
		cy.basePageSetup({
			isLoggedIn: false,
		});

		cy.visit(route('/'));
		cy.url().should('eq', route('/login'));

		cy.getPageData().then(() => {
			// Login
			cy.intercept('POST', AuthenticationPaths.appUserLoginEndpoint(), {
				statusCode: 401,
				body: generateFailedResultDTO(),
			}).as('loginUnauthorized');
			cy.interceptAuthenticationStatus(false);

			// Type credentials
			cy.getCy('login-username-input').type('admin');
			cy.getCy('login-password-input').type('password');
			cy.getCy('login-remember-me-input').click();
			cy.getCy('login-submit-button').click();
			cy.wait('@loginUnauthorized');

			cy.url().should('eq', route('/login'));
			cy.getCy('login-invalid-credentials-alert').should('be.visible');

			// Submit 3 times to lock account
			cy.getCy('login-submit-button').click();
			cy.wait('@loginUnauthorized');
			cy.getCy('login-submit-button').click();
			cy.wait('@loginUnauthorized');

			cy.intercept('POST', AuthenticationPaths.appUserLoginEndpoint(), {
				statusCode: 403,
				body: generateFailedResultDTO(),
			}).as('loginLocked');

			cy.getCy('login-submit-button').click();
			cy.wait('@loginLocked');

			cy.url().should('eq', route('/login'));
			cy.getCy('login-locked-out-alert').should('be.visible');
		});
	});
});
