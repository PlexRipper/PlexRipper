import { route } from '@fixtures/baseE2E';
import { generateResultDTO } from '@mock';
import type { SettingsModelDTO } from '@dto';
import { SettingsPaths } from '@api-urls';

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
			cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');
			// Change language
			cy.getCy('language-selector').click();
			cy.getCy('option-de-DE').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.languageSettings.language).to.equal('de-DE');
			});
		});
	});

	it('Should change short date format when short date selector is changed', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');
			// Change short date format
			cy.getCy('short-date-format').click();
			cy.getCy('option-yyyy-MM-dd').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.dateTimeSettings.shortDateFormat).to.equal('yyyy-MM-dd');
			});
		});
	});

	it('Should change long date format when long date selector is changed', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');
			// Change long date format
			cy.getCy('long-date-format').click();
			cy.getCy('option-EEEE, MMMM dd, yyyy').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.dateTimeSettings.longDateFormat).to.equal('EEEE, MMMM dd, yyyy');
			});
		});
	});

	it('Should change time format when time selector is changed', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');
			// Change long date format
			cy.getCy('time-format').click();
			cy.getCy('option-pp').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.dateTimeSettings.timeFormat).to.equal('pp');
			});
		});
	});

	it('Should change relative dates when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');
			// Change relative dates
			cy.getCy('relative-date').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.dateTimeSettings.showRelativeDates).to.equal(true);
			});
		});
	});

	it('Should change download movie confirmation when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');
			// Change relative dates
			cy.getCy('ask-download-movie-confirmation').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.confirmationSettings.askDownloadMovieConfirmation).to.equal(false);
			});
		});
	});

	it('Should change download tv-show confirmation when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');
			// Change relative dates
			cy.getCy('ask-download-tvshow-confirmation').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.confirmationSettings.askDownloadTvShowConfirmation).to.equal(false);
			});
		});
	});

	it('Should change download season confirmation when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');
			// Change relative dates
			cy.getCy('ask-download-season-confirmation').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.confirmationSettings.askDownloadSeasonConfirmation).to.equal(false);
			});
		});
	});

	it('Should change download episode confirmation when toggle is clicked', () => {
		cy.getPageData().then((pageData) => {
			// Settings update call
			cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(pageData.settings),
			}).as('settingsUpdate');
			// Change relative dates
			cy.getCy('ask-download-episode-confirmation').click();

			cy.wait('@settingsUpdate').then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				expect(settings.confirmationSettings.askDownloadEpisodeConfirmation).to.equal(false);
			});
		});
	});
});
