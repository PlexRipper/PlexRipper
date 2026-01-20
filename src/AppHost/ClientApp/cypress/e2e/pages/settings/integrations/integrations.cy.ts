import { route } from '@fixtures';
import type { SettingsModelDTO } from '@dto';
import { TestConnectionStatus } from '@dto';
import { IntegrationPaths } from '@api/generated/Integration';
import { generateResultDTO } from '@mock';

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
				body: generateResultDTO(
					{
						result: TestConnectionStatus.Success,
					}),
			});

			cy.contains('Sonarr').should('be.visible');
			cy.contains('Radarr').should('be.visible');
			cy.get('.q-stepper').should('have.length.at.least', 2);

			cy.getCy('sonarr-base-url-input').clear().type(sonarrBaseUrl);
			cy.getCy('sonarr-api-key-input').clear().type(sonarrApiKey);

			// Test Sonarr connection
			cy.getCy('test-sonarr-connection-button').click();

			// Verify values persisted in settings
			cy.awaitSettingsUpdate().then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.integrationsSettings.sonarr.sonarrBaseUrl).to.equal(sonarrBaseUrl);
				expect(settings.integrationsSettings.sonarr.sonarrApiKey).to.equal(sonarrApiKey);
			});

			// Verify success alert and stepper state
			cy.getCy('test-sonarr-connection-status-alert').should('be.visible');
			cy.getCy('test-sonarr-configuration-status-alert').should('not.be.visible');

			// Mock configure endpoint
			cy.intercept('POST', IntegrationPaths.configureSonarrIntegrationEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(null),
			});

			// Configure Sonarr integration
			cy.getCy('configure-sonarr-button').click();

			// Verify successful configuration
			cy.getCy('test-sonarr-configuration-status-alert').should('be.visible');

			// ===== Configure Radarr Integration =====

			// Scroll to Radarr section
			cy.contains('Radarr').scrollIntoView();

			// Enter Radarr credentials
			const radarrBaseUrl = 'http://localhost:7878';
			const radarrApiKey = 'b12b33b547615f26c8f57875c23936ec';
			cy.getCy('radarr-base-url-input').clear().type(radarrBaseUrl);
			cy.getCy('radarr-api-key-input').clear().type(radarrApiKey);

			// Verify values persisted in settings
			cy.awaitSettingsUpdate().then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.integrationsSettings.radarr.radarrBaseUrl).to.equal(radarrBaseUrl);
				expect(settings.integrationsSettings.radarr.radarrApiKey).to.equal(radarrApiKey);
			});

			// Test Radarr connection successfully
			cy.interceptNoQuery('GET', IntegrationPaths.testConnectionToRadarrEndpoint({
				url: radarrBaseUrl,
				apiKey: radarrApiKey,
			}), {
				statusCode: 200,
				body: generateResultDTO({ result: TestConnectionStatus.Success }),
			}).as('testRadarr');

			cy.getCy('test-radarr-connection-button').click();
			cy.wait('@testRadarr');

			// Verify success alert
			cy.get('.q-alert').last().should('contain', 'success');

			// Navigate to configure step for Radarr
			cy.contains('Configure').last().click();

			// Configure Radarr integration
			cy.intercept('POST', IntegrationPaths.configureRadarrIntegrationEndpoint(), {
				statusCode: 200,
				body: {
					isSuccess: true,
				},
			}).as('configureRadarr');

			cy.getCy('configure-radarr-button').click();
			cy.wait('@configureRadarr');

			// Verify successful configuration
			cy.get('.q-alert').last().should('contain', 'success');

			// Verify both integrations are fully configured
			cy.get('.q-stepper__step--done').should('have.length.at.least', 2);
		});
	});

	xit('Should handle integration failures with invalid credentials and retry workflow', () => {
		cy.getPageData().then(() => {
			// Verify page loaded
			cy.contains('Sonarr').should('be.visible');
			cy.contains('Radarr').should('be.visible');

			// ===== Attempt Sonarr with invalid API key =====

			// Enter Sonarr credentials with invalid API key
			cy.getCy('sonarr-base-url-input').clear().type('http://localhost:8989');
			cy.getCy('sonarr-api-key-input').clear().type('invalid-sonarr-key');

			// Mock failed test connection - Invalid API Key
			cy.intercept('POST', '/api/integrations/sonarr/test', {
				statusCode: 200,
				body: {
					data: {
						status: TestConnectionStatus.InvalidApiKey,
					},
					isSuccess: false,
				},
			}).as('testSonarrFail');

			cy.getCy('test-sonarr-connection-button').click();
			cy.wait('@testSonarrFail');

			// Verify error state and message
			cy.get('.q-stepper__step--error').should('exist');
			cy.get('.q-alert').first().should('be.visible');

			// Verify configure step is disabled
			cy.contains('Configure').first().click();
			// Should remain on connection step or configure button should be disabled

			// ===== Retry Sonarr with correct credentials =====

			// Fix the API key
			cy.getCy('sonarr-api-key-input').clear().type('a02a22a436504e15b7e46764b12825db');

			// Mock successful test connection
			cy.intercept('POST', '/api/integrations/sonarr/test', {
				statusCode: 200,
				body: {
					data: {
						status: TestConnectionStatus.Success,
					},
					isSuccess: true,
				},
			}).as('testSonarrSuccess');

			cy.getCy('test-sonarr-connection-button').click();
			cy.wait('@testSonarrSuccess');

			// Verify success
			cy.get('.q-alert').first().should('contain', 'success');
			cy.get('.q-stepper__step--done').should('exist');

			// ===== Attempt Radarr with connection failure =====

			// Scroll to Radarr section
			cy.contains('Radarr').scrollIntoView();

			// Enter Radarr credentials with wrong URL (connection will fail)
			cy.getCy('radarr-base-url-input').clear().type('http://localhost:9999');
			cy.getCy('radarr-api-key-input').clear().type('b12b33b547615f26c8f57875c23936ec');

			// Mock connection failed
			cy.intercept('POST', '/api/integrations/radarr/test', {
				statusCode: 200,
				body: {
					data: {
						status: TestConnectionStatus.ConnectionFailed,
					},
					isSuccess: false,
				},
			}).as('testRadarrFail');

			cy.getCy('test-radarr-connection-button').click();
			cy.wait('@testRadarrFail');

			// Verify error message
			cy.get('.q-alert').last().should('be.visible');

			// ===== Retry Radarr with correct URL =====

			// Fix the URL
			cy.getCy('radarr-base-url-input').clear().type('http://localhost:7878');

			// Mock successful connection
			cy.intercept('POST', '/api/integrations/radarr/test', {
				statusCode: 200,
				body: {
					data: {
						status: TestConnectionStatus.Success,
					},
					isSuccess: true,
				},
			}).as('testRadarrSuccess');

			cy.getCy('test-radarr-connection-button').click();
			cy.wait('@testRadarrSuccess');

			// Verify success
			cy.get('.q-alert').last().should('contain', 'success');

			// Navigate to configure and complete setup
			cy.contains('Configure').last().click();

			cy.intercept('POST', '/api/integrations/radarr/configure', {
				statusCode: 200,
				body: {
					isSuccess: true,
				},
			}).as('configureRadarr');

			cy.getCy('configure-radarr-button').click();
			cy.wait('@configureRadarr');

			// Verify successful configuration
			cy.get('.q-alert').last().should('contain', 'success');

			// ===== Complete Sonarr configuration =====

			// Scroll back to Sonarr
			cy.contains('Sonarr').scrollIntoView();

			// Navigate to Sonarr configure step
			cy.contains('Configure').first().click();

			// Configure Sonarr
			cy.intercept('POST', '/api/integrations/sonarr/configure', {
				statusCode: 200,
				body: {
					isSuccess: true,
				},
			}).as('configureSonarr');

			cy.getCy('configure-sonarr-button').click();
			cy.wait('@configureSonarr');

			// Verify successful configuration
			cy.get('.q-alert').first().should('contain', 'success');

			// Verify both integrations are fully configured after error recovery
			cy.get('.q-stepper__step--done').should('have.length.at.least', 2);
		});
	});
});
