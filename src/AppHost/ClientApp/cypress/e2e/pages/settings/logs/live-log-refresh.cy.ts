import { route } from '@fixtures';
import { DebugPaths } from '@api-urls';
import { generateResultDTO } from '@mock';
import { LogSeverity, type LiveLogEventDTO } from '@dto';

describe('Live log history refresh', () => {
	beforeEach(() => {
		cy.hubClear();
		// Publish to the current app document rather than the mock package's retained window.
		Cypress.Commands.overwrite('hubPublish', (_original, hubName, messageType, payload) =>
			cy.window().then((autWindow) => {
				const hub = autWindow['cypress-signalr-mock']?.mocks.find((mock) => mock.name === hubName);
				if (!hub) {
					throw new Error(`SignalR mock hub ${hubName} is not ready in the current page`);
				}
				hub.publish(messageType, payload);
			}),
		);
	});

	it('Should show each event once after overlapping snapshots while preserving separate same-text events', () => {
		cy.basePageSetup({ isLoggedIn: true, plexAccountCount: 0, plexServerCount: 0 }).then(() => {
			const cached: LiveLogEventDTO = {
				sequence: 100,
				timestamp: '2026-10-18T12:00:00Z',
				level: LogSeverity.Information,
				message: 'Library sync started',
			};
			const firstLive: LiveLogEventDTO = { ...cached, sequence: 101, message: 'Library sync finished' };
			const secondLive: LiveLogEventDTO = { ...firstLive, sequence: 102 };
			let snapshot = [cached];
			cy.intercept('GET', DebugPaths.getAllLogsEndpoint(), (request) => {
				request.alias = snapshot.length === 1 ? 'initialLogs' : 'refreshedLogs';
				request.reply({ statusCode: 200, body: generateResultDTO(snapshot) });
			});

			cy.visit(route('/settings/logs'));
			cy.wait('@initialLogs').its('response.statusCode').should('equal', 200);
			cy.getCy('live-log-viewer').find('.live-log-viewer__message').should('have.length', 1)
				.and('have.text', 'Library sync started');
			cy.hubPublish('logs', 'LogEvent', firstLive);
			cy.hubPublish('logs', 'LogEvent', secondLive);
			cy.getCy('live-log-viewer').find('.live-log-viewer__message').should('have.length', 3);
			cy.getCy('live-log-viewer').find('.live-log-viewer__entry-checkbox').should(($entries) => {
				expect([...$entries].map((entry) => entry.getAttribute('aria-label')?.split(' ').pop())).to.deep.equal([
					'100', '101', '102',
				]);
			});

			cy.then(() => {
				snapshot = [cached, firstLive];
			});
			cy.getCy('live-log-viewer').find('button').filter(':has(.mdi-refresh)').click();
			cy.wait('@refreshedLogs').its('response.body.value').should('have.length', 2);
			cy.getCy('live-log-viewer').find('.live-log-viewer__message').should('have.length', 3);
			cy.getCy('live-log-viewer').find('button').filter(':has(.mdi-refresh)').click();
			cy.wait('@refreshedLogs').its('response.body.value').should('have.length', 2);
			cy.getCy('live-log-viewer').find('.live-log-viewer__message').should(($messages) => {
				expect([...$messages].map((entry) => entry.textContent)).to.deep.equal([
					'Library sync started', 'Library sync finished', 'Library sync finished',
				]);
			});

			cy.then(() => {
				snapshot = [cached, firstLive, secondLive];
			});
			cy.reload();
			cy.wait('@refreshedLogs').its('response.body.value').should('have.length', 3);
			cy.getCy('live-log-viewer').find('.live-log-viewer__message').should(($messages) => {
				expect([...$messages].map((entry) => entry.textContent)).to.deep.equal([
					'Library sync started', 'Library sync finished', 'Library sync finished',
				]);
			});
		});
	});
});
