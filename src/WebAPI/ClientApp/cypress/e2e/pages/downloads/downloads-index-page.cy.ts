import { cloneDeep } from 'lodash-es';
import prettyBytes from 'pretty-bytes';
import { route } from '@fixtures/baseE2E';
import { DownloadStatus, MessageTypes } from '@dto/mainApi';

describe('Downloads page', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexLibraryCount: 5,
			movieDownloadTask: 3,
		});

		cy.visit(route('/downloads')).as('downloadsPage');
	});

	it('Should update the download task row when the download process is updated', () => {
		cy.url().should('eq', route('/downloads'));

		cy.getPageData().then((data) => {
			const downloadTasks = data.serverDownloadProgress[0].downloads;
			Cypress._.times(downloadTasks.length, (downloadTaskIndex) => {
				const iterations = 10;
				Cypress._.times(iterations + 1, (i) => {
					const updatedProgress = cloneDeep(data.serverDownloadProgress[0]);
					const downloadTask = updatedProgress.downloads[downloadTaskIndex];
					const dataReceived = i * (downloadTask.dataTotal / iterations);
					const downloadSpeed = downloadTask.dataTotal / iterations;
					const timeRemaining = iterations - i;
					const percentage = i * 10;
					const status = percentage === 100 ? DownloadStatus.Completed : DownloadStatus.Downloading;
					updatedProgress.downloads = [
						{
							...downloadTask,
							percentage,
							status,
							timeRemaining,
							dataReceived,
							downloadSpeed,
						},
					];
					cy.hubPublish('progress', MessageTypes.ServerDownloadProgress, updatedProgress);
					cy.getCy(`column-status-${downloadTask.id}`).should('have.text', status);
					cy.getCy(`column-dataReceived-${downloadTask.id}`).should('have.text', prettyBytes(dataReceived));
					cy.getCy(`column-dataTotal-${downloadTask.id}`).should('have.text', prettyBytes(downloadTask.dataTotal));
					cy.getCy(`column-downloadSpeed-${downloadTask.id}`).should('have.text', prettyBytes(downloadSpeed) + `/s`);
					cy.getCy(`column-percentage-${downloadTask.id}`).should('have.text', `${percentage}%`);
					cy.getCy(`column-timeRemaining-${downloadTask.id}`).should(
						'have.text',
						timeRemaining > 0 ? String(timeRemaining).padStart(2, '0') : '-',
					);
				});
			});
		});
	});
});
