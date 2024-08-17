import './commands';
import Log from 'consola';
import { basePageSetup, route, type IBasePageSetupResult } from '@fixtures/baseE2E';
import { generateJobStatusUpdate, type MockConfig } from '@mock';
import { JobStatus, JobTypes, MessageTypes, type PlexServerDTO } from '@dto';

Cypress.Commands.add('basePageSetup', (config: Partial<MockConfig> = {}) => basePageSetup(config).as('pageData'));

Cypress.Commands.add('getPageData', () =>
	cy
		.get('[data-cy="page-load-completed"]', { timeout: 10000 }) // Wait for page load
		.get('@pageData')
		.then((x) => {
			const data = x as unknown as IBasePageSetupResult;
			Log.info('PageData generated:', data);
			cy.log('PageData generated:', data);
			return cy.wrap(data);
		}),
);

Cypress.Commands.add('visitEmptyPage', () => cy.visit(route('/empty')).as('emptyPage'));

Cypress.Commands.add('getCy', (selector: string) => cy.get(`[data-cy="${selector}"]`));

Cypress.Commands.add(
	'hubPublishJobStatusUpdate',
	<T>(type: JobTypes, status: JobStatus, data: T) => {
		const msg = generateJobStatusUpdate({
			jobType: type,
			jobStatus: status,
			data,
		});
		cy.hubPublish(
			'progress',
			MessageTypes.JobStatusUpdate,
			msg,
		).log('JobStatusUpdate', type, status, msg);
	},
);

Cypress.Commands.add('hubPublishCheckPlexServerConnectionsJob', (servers: PlexServerDTO[]) =>
	cy
		.hubPublishJobStatusUpdate(
			JobTypes.InspectPlexServerJob,
			JobStatus.Started,
			servers.map((x) => x.id),
		)
		.getCy('check-server-connection-dialog')
		.should('exist')
		.and('be.visible'),
);
