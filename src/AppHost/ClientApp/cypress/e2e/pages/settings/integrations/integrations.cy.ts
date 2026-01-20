import { route } from '@fixtures';
import type { SettingsModelDTO } from '@dto';
import { TestConnectionStatus } from '@dto';
import { IntegrationPaths } from '@api/generated/Integration';
import { generateFailedResultDTO, generateResultDTO } from '@mock';

describe('Configure Integrations Workflow', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 0,
			plexServerCount: 0,
		});

		cy.visit(route('/settings/integrations'));
	});

	it('Should complete full Sonarr and Radarr integration setup workflow with successful connections', () => {
		cy.getPageData().then(() => {
			// Enter Sonarr credentials
			const sonarrBaseUrl = 'http://localhost:8989';
			const sonarrApiKey = 'a02a22a436504e15b7e46764b12825db';

			// Mock the test connection endpoint BEFORE any page interaction
			cy.interceptNoQuery('GET', IntegrationPaths.testConnectionToSonarrEndpoint({
				url: sonarrBaseUrl,
				apiKey: sonarrApiKey,
			}), {
				statusCode: 200,
				body: generateResultDTO({ result: TestConnectionStatus.Success }),
			}).as('testSonarr');

			cy.contains('Sonarr').should('be.visible');
			cy.contains('Radarr').should('be.visible');
			cy.get('.q-stepper').should('have.length.at.least', 2);

			cy.getCy('sonarr-base-url-input').clear().type(sonarrBaseUrl);
			cy.getCy('sonarr-api-key-input').clear().type(sonarrApiKey);

			// Test Sonarr connection
			cy.getCy('test-sonarr-connection-button').click();
			cy.wait('@testSonarr');

			// Verify values persisted in settings
			cy.awaitSettingsUpdate().then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.integrationsSettings.sonarr.sonarrBaseUrl).to.equal(sonarrBaseUrl);
				expect(settings.integrationsSettings.sonarr.sonarrApiKey).to.equal(sonarrApiKey);
			});

			// Verify success alert and stepper state
			cy.getCy('test-sonarr-connection-status-alert').should('be.visible');
			cy.getCy('test-sonarr-configuration-status-alert').should('not.exist');

			// Mock configure endpoint before any configure interaction
			cy.intercept('POST', IntegrationPaths.configureSonarrIntegrationEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(null),
			}).as('configureSonarr');

			// Configure Sonarr integration
			cy.getCy('configure-sonarr-button').scrollIntoView().should('be.visible').click();
			cy.wait('@configureSonarr');

			// Verify successful configuration
			cy.getCy('test-sonarr-configuration-status-alert').should('be.visible');

			// ===== Configure Radarr Integration =====

			// Enter Radarr credentials
			const radarrBaseUrl = 'http://localhost:7878';
			const radarrApiKey = 'b12b33b547615f26c8f57875c23936ec';

			// Mock the test connection endpoint BEFORE any Radarr interaction
			cy.interceptNoQuery('GET', IntegrationPaths.testConnectionToRadarrEndpoint({
				url: radarrBaseUrl,
				apiKey: radarrApiKey,
			}), {
				statusCode: 200,
				body: generateResultDTO({ result: TestConnectionStatus.Success }),
			}).as('testRadarr');

			cy.getCy('radarr-base-url-input').clear().type(radarrBaseUrl);
			cy.getCy('radarr-api-key-input').clear().type(radarrApiKey);

			// Test Radarr connection
			cy.getCy('test-radarr-connection-button').should('be.visible').click();
			cy.wait('@testRadarr');

			// Verify values persisted in settings
			cy.awaitSettingsUpdate().then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.integrationsSettings.radarr.radarrBaseUrl).to.equal(radarrBaseUrl);
				expect(settings.integrationsSettings.radarr.radarrApiKey).to.equal(radarrApiKey);
			});

			// Verify success alert and stepper state
			cy.getCy('test-radarr-connection-status-alert').should('be.visible');
			cy.getCy('test-radarr-configuration-status-alert').should('not.exist');

			// Mock configure endpoint before any configure interaction
			cy.intercept('POST', IntegrationPaths.configureRadarrIntegrationEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(null),
			}).as('configureRadarr');

			// Configure Radarr integration
			cy.getCy('configure-radarr-button').scrollIntoView().should('be.visible').click();
			cy.wait('@configureRadarr');

			// Verify successful configuration
			cy.getCy('test-radarr-configuration-status-alert').should('be.visible');
		});
	});
});
