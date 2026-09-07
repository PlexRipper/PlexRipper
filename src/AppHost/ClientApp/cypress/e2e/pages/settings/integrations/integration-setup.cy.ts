import { IntegrationPaths } from '@api/generated/Integration';
import { route } from '@fixtures';
import {
	IntegrationProvisioningState,
	IntegrationSetupProgressStage,
	IntegrationType,
	MessageTypes,
	TestConnectionStatus,
	type IntegrationSetupProgressDTO,
	type IntegrationSummary,
	type SonarrIntegrationDTO,
} from '@dto';
import { generateResultDTO } from '@mock';

const integrationId = '00000000-0000-0000-0000-000000000001';
const summary: IntegrationSummary = {
	id: integrationId,
	type: IntegrationType.Sonarr,
	name: 'Sonarr',
	baseUrl: 'http://localhost:8989',
	category: 'sonarr',
	provisioningState: IntegrationProvisioningState.Unconfigured,
	lastConnectionTestStatus: TestConnectionStatus.Unknown,
	downloadFolderId: null,
	externalDownloadClientId: null,
	externalIndexerId: null,
};
const detail: SonarrIntegrationDTO = {
	id: integrationId,
	name: summary.name,
	url: summary.baseUrl,
	apiKey: 'sonarr-api-key',
	category: summary.category,
	downloadFolderId: null,
	provisioningState: IntegrationProvisioningState.Configured,
	lastConnectionTestStatus: TestConnectionStatus.Unknown,
};
const setupStages = [
	{ stage: IntegrationSetupProgressStage.Connecting, selector: 'connect' },
	{ stage: IntegrationSetupProgressStage.DownloadClient, selector: 'download-client' },
	{ stage: IntegrationSetupProgressStage.Indexer, selector: 'indexer' },
	{ stage: IntegrationSetupProgressStage.Validation, selector: 'validation' },
	{ stage: IntegrationSetupProgressStage.Done, selector: 'done' },
] as const;

function publishProgress(
	stage: IntegrationSetupProgressStage,
	isSuccess: boolean,
	error?: string,
	isRunning = false,
	targetIntegrationId = integrationId,
): Cypress.Chainable {
	return cy.hubPublish('progress', MessageTypes.IntegrationSetupProgress, {
		integrationId: targetIntegrationId,
		stage,
		isRunning,
		isSuccess,
		error,
	} satisfies IntegrationSetupProgressDTO);
}

function openSetupDialog(): void {
	cy.getCy('integration-card').click();
	cy.wait('@getSonarr');
	cy.getCy('integration-setup').click();
	cy.getCy('integration-setup-dialog').should('be.visible');
}

describe('Integration setup progress', () => {
	beforeEach(() => {
		cy.basePageSetup({ plexAccountCount: 0, plexServerCount: 0 });
		cy.intercept('GET', IntegrationPaths.getIntegrationsEndpoint(), {
			statusCode: 200,
			body: generateResultDTO([summary]),
		}).as('getIntegrations');
		cy.intercept('GET', `/api/Integration/Sonarr/${integrationId}`, {
			statusCode: 200,
			body: generateResultDTO(detail),
		}).as('getSonarr');
		cy.intercept('POST', IntegrationPaths.setupSonarrIntegrationEndpoint(integrationId), {
			statusCode: 200,
			body: generateResultDTO(detail),
		}).as('setupSonarr');
		cy.visit(route('/settings/integrations'));
		cy.wait('@getIntegrations');
		openSetupDialog();
	});

	it('completes every setup step', () => {
		for (const { stage, selector } of setupStages) {
			publishProgress(stage, true);
			cy.getCy(`integration-setup-step-${selector}`).should('have.attr', 'data-status', 'success');
		}

		cy.getCy('integration-setup-step-validation').should('contain.text', 'Sonarr setup validated.');
		cy.getCy('integration-setup-step-done').should('contain.text', 'Integration setup complete.');
		cy.wait('@setupSonarr').its('request.method').should('eq', 'POST');
	});

	for (const { stage, selector } of setupStages) {
		it(`shows ${selector} as running`, () => {
			publishProgress(stage, false, undefined, true);
			cy.getCy(`integration-setup-step-${selector}`).should('have.attr', 'data-status', 'running');
		});
	}

	it('ignores setup progress for another integration', () => {
		publishProgress(
			IntegrationSetupProgressStage.Validation,
			true,
			undefined,
			false,
			'00000000-0000-0000-0000-000000000099',
		);

		for (const { selector } of setupStages) {
			cy.getCy(`integration-setup-step-${selector}`).should('have.attr', 'data-status', 'pending');
		}
	});

	it('does not complete Done for an unknown stage', () => {
		publishProgress('Unexpected' as IntegrationSetupProgressStage, true);

		for (const { selector } of setupStages) {
			cy.getCy(`integration-setup-step-${selector}`).should('have.attr', 'data-status', 'pending');
		}
	});

	it('clears a validation error when the stage is retried successfully', () => {
		publishProgress(IntegrationSetupProgressStage.Validation, false, 'Validation failed');
		cy.getCy('integration-setup-step-validation')
			.should('have.attr', 'data-status', 'error')
			.and('contain.text', 'Validation failed');

		publishProgress(IntegrationSetupProgressStage.Validation, false, undefined, true);
		cy.getCy('integration-setup-step-validation')
			.should('have.attr', 'data-status', 'running')
			.and('not.contain.text', 'Validation failed');

		publishProgress(IntegrationSetupProgressStage.Validation, true);
		cy.getCy('integration-setup-step-validation')
			.should('have.attr', 'data-status', 'success')
			.and('contain.text', 'Sonarr setup validated.');
	});

	for (const [failedIndex, failedStage] of setupStages.entries()) {
		it(`shows a clear error when ${failedStage.selector} fails`, () => {
			for (const { stage, selector } of setupStages.slice(0, failedIndex)) {
				publishProgress(stage, true);
				cy.getCy(`integration-setup-step-${selector}`).should('have.attr', 'data-status', 'success');
			}

			const error = `${failedStage.selector} setup failed clearly`;
			publishProgress(failedStage.stage, false, error);
			cy.getCy(`integration-setup-step-${failedStage.selector}`)
				.should('have.attr', 'data-status', 'error')
				.and('contain.text', error);

			for (const { selector } of setupStages.slice(failedIndex + 1)) {
				cy.getCy(`integration-setup-step-${selector}`).should('have.attr', 'data-status', 'pending');
			}
		});
	}
});
