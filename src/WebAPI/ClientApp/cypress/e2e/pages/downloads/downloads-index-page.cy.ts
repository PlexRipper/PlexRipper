import { cloneDeep } from 'lodash-es';
import prettyBytes from 'pretty-bytes';
import { route } from '@fixtures/baseE2E';
import { DownloadStatus, MessageTypes } from '@dto';

describe('Downloads page', () => {
	it('Should update the download task row when the download process is updated', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 5,
			movieDownloadTask: 3,
		});

		cy.visit(route('/downloads')).as('downloadsPage');
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

	it('Should handle huge list of download tasks when navigating the downloads table paginator', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 5,
			tvShowDownloadTask: 100,
		});
		cy.visit(route('/downloads')).as('downloadsPage');
		cy.url().should('eq', route('/downloads'));
		cy.getPageData().then((data) => {
			const downloads = data.serverDownloadProgress[0].downloads;
			Cypress._.times(10, (i) => {
				cy.get(`:nth-child(1) > .p-paginator > .p-paginator-pages > [aria-label="${i + 1}"]`).click();
				// Ensure the table content changes by checking the first row title
				cy.getCy(`column-title-${downloads[i * 10 + 1].id}`).should('have.text', downloads[i * 10 + 1].title);
			});
		});

		cy.url().should('eq', route('/downloads'));
	});

	it('Should open details dialog when clicking on the details action button next to a download task row', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 5,
			movieDownloadTask: 5,
			setDownloadDetails: true,
		});
		cy.visit(route('/downloads')).as('downloadsPage');
		cy.url().should('eq', route('/downloads'));
		cy.getPageData().then((data) => {
			const downloadTask = data.detailDownloadTasks[0];
			cy.getCy(`column-actions-details-${downloadTask.id}`).click();
			cy.getCy('download-details-dialog-status').should('contain.text', downloadTask.status);
			cy.getCy('download-details-dialog-file-name').should('contain.text', downloadTask.fileName);
			cy.getCy('download-details-dialog-download-path').should('contain.text', downloadTask.downloadDirectory);
			cy.getCy('download-details-dialog-destination-path').should('contain.text', downloadTask.destinationDirectory);
			cy.getCy('download-details-dialog-download-url').should('contain.text', downloadTask.downloadUrl);
		});
	});
});
