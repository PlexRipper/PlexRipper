import type {
	CheckAllConnectionStatusUpdateDTO,
	JobStatus,
	PlexServerConnectionDTO,
	PlexServerDTO,
} from '@dto';
import {
	JobTypes,
	MessageTypes,
} from '@dto';
import { generateJobStatusUpdate } from '@factories';

Cypress.Commands.add('hubPublishJobStatusUpdate', <T>(type: JobTypes, status: JobStatus, data: T) => {
	const msg = generateJobStatusUpdate({
		jobType: type,
		jobStatus: status,
		data,
	});
	cy.hubPublish('progress', MessageTypes.JobStatusUpdate, msg).log('JobStatusUpdate', type, status, msg);
});

Cypress.Commands.add(
	'hubPublishCheckPlexServerConnectionsJob',
	(status: JobStatus, servers: PlexServerDTO[], connections: PlexServerConnectionDTO[]) =>
		cy.hubPublishJobStatusUpdate<CheckAllConnectionStatusUpdateDTO>(JobTypes.CheckAllConnectionsStatusByPlexServerJob, status, {
			plexServersWithConnectionIds: servers.reduce(
				(acc, server) => {
					acc[server.id] = connections.filter((x) => x.plexServerId === server.id).map((x) => x.id);
					return acc;
				},
				{} as Record<string, number[]>,
			),
		}),
);

Cypress.Commands.add('hubPublishInspectPlexServerJob', (status: JobStatus, plexServerIds: number[]) =>
	cy.hubPublishJobStatusUpdate<number[]>(JobTypes.InspectPlexServerJob, status, plexServerIds),
);
