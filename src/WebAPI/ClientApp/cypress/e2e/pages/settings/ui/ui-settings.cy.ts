import { APIRoute, apiRoute, route } from '@fixtures/baseE2E';
import { generateResultDTO } from '@mock';
import { SettingsModelDTO } from '@dto/mainApi';

describe('Change UI settings', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
		});

		cy.visit(route('/settings/ui')).as('setupPage');
	});

	it('Should change language to German when language selector is changed', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), (req) => {
				req.reply({
					statusCode: 200,
					body: generateResultDTO(pageData.settings),
				});
				expect((req.body as SettingsModelDTO).languageSettings.language).to.equal('de-DE');
			});
			// Change language
			cy.getCy('language-selector').click();
			cy.getCy('option-de-DE').click();
		});
	});

	it('Should change short date format when short date selector is changed', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), (req) => {
				req.reply({
					statusCode: 200,
					body: generateResultDTO(pageData.settings),
				});
				expect((req.body as SettingsModelDTO).dateTimeSettings.shortDateFormat).to.equal('yyyy-MM-dd');
			});
			// Change short date format
			cy.getCy('short-date-format').click();
			cy.getCy('option-yyyy-MM-dd').click();
		});
	});

	it('Should change long date format when long date selector is changed', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), (req) => {
				req.reply({
					statusCode: 200,
					body: generateResultDTO(pageData.settings),
				});
				expect((req.body as SettingsModelDTO).dateTimeSettings.longDateFormat).to.equal('EEEE, MMMM dd, yyyy');
			});
			// Change long date format
			cy.getCy('long-date-format').click();
			cy.getCy('option-EEEE, MMMM dd, yyyy').click();
		});
	});

	it('Should change time format when time selector is changed', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), (req) => {
				req.reply({
					statusCode: 200,
					body: generateResultDTO(pageData.settings),
				});
				expect((req.body as SettingsModelDTO).dateTimeSettings.timeFormat).to.equal('pp');
			});
			// Change long date format
			cy.getCy('time-format').click();
			cy.getCy('option-pp').click();
		});
	});

	it('Should change relative dates when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), (req) => {
				req.reply({
					statusCode: 200,
					body: generateResultDTO(pageData.settings),
				});
				expect((req.body as SettingsModelDTO).dateTimeSettings.showRelativeDates).to.equal(true);
			});
			// Change relative dates
			cy.getCy('relative-date').click();
		});
	});

	it('Should change download movie confirmation when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), (req) => {
				req.reply({
					statusCode: 200,
					body: generateResultDTO(pageData.settings),
				});
				expect((req.body as SettingsModelDTO).confirmationSettings.askDownloadMovieConfirmation).to.equal(false);
			});
			// Change relative dates
			cy.getCy('ask-download-movie-confirmation').click();
		});
	});

	it('Should change download tv-show confirmation when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), (req) => {
				req.reply({
					statusCode: 200,
					body: generateResultDTO(pageData.settings),
				});
				expect((req.body as SettingsModelDTO).confirmationSettings.askDownloadTvShowConfirmation).to.equal(false);
			});
			// Change relative dates
			cy.getCy('ask-download-tvshow-confirmation').click();
		});
	});

	it('Should change download season confirmation when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), (req) => {
				req.reply({
					statusCode: 200,
					body: generateResultDTO(pageData.settings),
				});
				expect((req.body as SettingsModelDTO).confirmationSettings.askDownloadSeasonConfirmation).to.equal(false);
			});
			// Change relative dates
			cy.getCy('ask-download-season-confirmation').click();
		});
	});

	it('Should change download episode confirmation when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), (req) => {
				req.reply({
					statusCode: 200,
					body: generateResultDTO(pageData.settings),
				});
				expect((req.body as SettingsModelDTO).confirmationSettings.askDownloadEpisodeConfirmation).to.equal(false);
			});
			// Change relative dates
			cy.getCy('ask-download-episode-confirmation').click();
		});
	});
});
