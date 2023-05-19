import 'cypress-signalr-mock';

import '@testing-library/cypress/add-commands';

// Source: https://stackoverflow.com/a/63519375/8205497
Cypress.on('uncaught:exception', (err) => {
	/* returning false here prevents Cypress from failing the test */
	if (err.message.includes('ResizeObserver loop limit exceeded')) {
		return false;
	}
});
