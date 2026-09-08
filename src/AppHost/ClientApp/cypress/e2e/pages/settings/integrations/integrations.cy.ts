import { route } from '@fixtures';
import { IntegrationPaths } from '@api/generated/Integration';
import { IntegrationProvisioningState, IntegrationType, TestConnectionStatus, type IntegrationSummary } from '@dto';
import { generateResultDTO } from '@mock';

describe('Manage integrations', () => {
	const sonarr: IntegrationSummary = {
		id: '00000000-0000-0000-0000-000000000001',
		type: IntegrationType.Sonarr,
		name: 'Sonarr',
		baseUrl: 'http://localhost:8989',
		category: 'sonarr',
		provisioningState: IntegrationProvisioningState.Configured,
		lastConnectionTestStatus: TestConnectionStatus.Unknown,
		downloadFolderId: 1,
		externalDownloadClientId: 1,
		externalIndexerId: 2,
	};

	beforeEach(() => {
		cy.basePageSetup({ plexAccountCount: 0, plexServerCount: 0 });
		cy.intercept('GET', IntegrationPaths.getIntegrationsEndpoint(), {
			statusCode: 200,
			body: generateResultDTO([sonarr]),
		}).as('getIntegrations');
		cy.visit(route('/settings/integrations'));
		cy.wait('@getIntegrations');
	});

	it('opens the Sonarr form directly from the add integration dialog', () => {
		cy.getCy('add-integration').click();
		cy.getCy('integration-dialog').should('be.visible');
		cy.getCy('integration-type-sonarr').click();
		cy.getCy('integration-name').should('be.visible');
		cy.getCy('integration-base-url').should('be.visible');
		cy.getCy('integration-arr-key').should('be.visible');
		cy.getCy('integration-download-folder').should('be.visible');
	});

	it('opens the typed edit form for an existing integration', () => {
		cy.intercept('GET', `/api/Integration/Sonarr/${sonarr.id}`, {
			statusCode: 200,
			body: generateResultDTO({
				id: sonarr.id,
				name: sonarr.name,
				url: sonarr.baseUrl,
				apiKey: 'sonarr-api-key',
				category: sonarr.category,
				downloadFolderId: null,
				provisioningState: sonarr.provisioningState,
				lastConnectionTestStatus: TestConnectionStatus.Unknown,
			}),
		}).as('getSonarr');

		cy.getCy('integration-edit').click();
		cy.wait('@getSonarr');
		cy.getCy('integration-name').should('have.value', sonarr.name);
		cy.getCy('integration-base-url').should('have.value', sonarr.baseUrl);
	});
});
