import { cy, beforeEach, afterEach, Cypress } from 'local-cypress';
import './commands';

// beforeEach(() => {
// 	const notMockedRequests = cy.stub().as('notMockedRequests');
// 	cy.intercept(Cypress.env('API_URL') + '/**', notMockedRequests);
// });
//
// /**
//  * This is a custom command to check if there are any requests that were not mocked.
//  */
// afterEach(() => {
// 	cy.get('@notMockedRequests')
// 		.then((stub) =>
// 			cy.wrap(
// 				stub
// 					// @ts-ignore
// 					.getCalls()
// 					.map((call) => call.args[0].url)
// 					.join(', '),
// 			),
// 		)
// 		.should('be.empty');
// });
