import type { BasePageSetupResult } from '@fixtures';
import type { MockConfig } from '@mock';
import { generateServerDownloadProgress, generateResultDTO, Seed, generateDownloadTask } from '@mock';
import { DownloadPaths } from '@api/api-paths';
import { headers } from '@fixtures';
import Convert from '@class/Convert';

export function setupMockDownloadTasksEndpoints(
	this: BasePageSetupResult,
	config: MockConfig,
): BasePageSetupResult {
	this.serverDownloadProgress = this.plexServers
		.map((server) =>
			generateServerDownloadProgress({
				plexServerId: server.id,
				plexLibraryId: -1,
				config,
				seed: new Seed(config.seed),
			}),
		)
		.flat();
	if (config.override.downloadTasks) {
		this.serverDownloadProgress = config.override.downloadTasks(this.serverDownloadProgress);
	}
	cy.intercept('GET', DownloadPaths.getAllDownloadTasksEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(this.serverDownloadProgress),
		...headers,
	}).then(() => {
		if (config.debugDisplayData) {
			cy.log('BasePageSetup -> downloadTasks', this.serverDownloadProgress);
		}
	});

	// DownloadDetails call
	if (config.setDownloadDetails) {
		for (const serverDownload of this.serverDownloadProgress) {
			const downloadTasks = this.serverDownloadProgress.flatMap((x) => x.downloads);
			for (const downloadTask of downloadTasks) {
				const generatedDownloadTask = generateDownloadTask({
					config,
					id: downloadTask.id,
					plexLibraryId: 1,
					plexServerId: serverDownload.id,
					type: Convert.toDownloadTaskType(downloadTask.mediaType),
					// partial: downloadTask,
				});
				this.detailDownloadTasks.push(generatedDownloadTask);
				cy.intercept('GET', DownloadPaths.getDownloadTaskByGuidEndpoint(downloadTask.id), {
					statusCode: 200,
					body: generateResultDTO(generatedDownloadTask),
					...headers,
				});
			}
		}
	}

	return this;
}
