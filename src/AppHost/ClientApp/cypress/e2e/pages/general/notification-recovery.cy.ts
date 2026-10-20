import { route } from '@fixtures';
import { NotificationPaths } from '@api-urls';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { NotificationLevel, type NotificationDTO } from '@dto';

const notifications: NotificationDTO[] = [1, 2].map((id) => ({
	id, level: NotificationLevel.Information, createdAt: '2026-10-19T00:00:00Z', message: `Notification ${id}`, hidden: false,
}));

describe('Notification action recovery', () => {
	beforeEach(() => {
		cy.basePageSetup({ plexAccountCount: 0, plexServerCount: 0 }).then(() => {
			cy.intercept('GET', NotificationPaths.getAllNotificationsEndpoint(), {
				statusCode: 200, body: generateResultDTO(notifications),
			}).as('notifications');
			cy.visit(route('/settings/ui'));
			cy.wait('@notifications');
			cy.getCy('notifications-button').filter(':visible').click();
			cy.getCy('notification-1').should('be.visible');
		});
	});

	it('Should retain a failed hide for retry and remove only the successful row', () => {
		let calls = 0;
		cy.intercept('PATCH', NotificationPaths.setNotificationVisibilityEndpoint(), (request) => {
			calls++;
			expect(request.body).to.deep.equal({ id: 1, hidden: true });
			request.reply(calls === 1
				? { statusCode: 500, body: generateFailedResultDTO({ statusCode: 500 }) }
				: { statusCode: 200, body: { isSuccess: true, statusCode: 200, errors: [], successes: [] } });
		}).as('hide');

		cy.getCy('notification-1').click();
		cy.wait('@hide').its('response.statusCode').should('eq', 500);
		cy.getCy('api-error').should('contain.text', 'HTTP 500');
		cy.getCy('close-alert-dialog').click();
		cy.getCy('notification-1').should('be.visible');
		cy.getCy('notification-2').should('be.visible');
		cy.getCy('notification-1').click();
		cy.wait('@hide').its('response.statusCode').should('eq', 200);
		cy.getCy('notification-1').should('not.exist');
		cy.getCy('notification-2').should('be.visible');
		cy.then(() => expect(calls).to.equal(2));
	});

	it('Should keep failed clear open and close the drawer only after successful retry', () => {
		let calls = 0;
		cy.intercept('DELETE', NotificationPaths.clearAllNotificationsEndpoint(), (request) => {
			calls++;
			request.reply(calls === 1
				? { statusCode: 500, body: generateFailedResultDTO({ statusCode: 500 }) }
				: { statusCode: 200, body: generateResultDTO({ count: 2 }) });
		}).as('clear');

		cy.getCy('clear-notifications').click();
		cy.wait('@clear').its('response.statusCode').should('eq', 500);
		cy.getCy('api-error').should('contain.text', 'HTTP 500');
		cy.getCy('close-alert-dialog').click();
		cy.get('.notification-drawer').should('be.visible');
		cy.getCy('notification-1').should('be.visible');
		cy.getCy('notification-2').should('be.visible');
		cy.getCy('clear-notifications').click();
		cy.wait('@clear').its('response.statusCode').should('eq', 200);
		cy.get('.notification-drawer').should('not.be.visible');
		cy.getCy('notifications-button').filter(':visible').click();
		cy.get('.notification-drawer').should('be.visible');
		cy.getCy('notification-1').should('not.exist');
		cy.getCy('notification-2').should('not.exist');
		cy.getCy('clear-notifications').should('not.exist');
		cy.then(() => expect(calls).to.equal(2));
	});
});
