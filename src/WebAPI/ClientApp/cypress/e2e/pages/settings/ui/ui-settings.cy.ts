import { route } from '@fixtures';
import type { SettingsModelDTO } from '@dto';

describe('Change UI settings', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 0,
			plexServerCount: 0,
		});

		cy.visit(route('/settings/ui'));
	});

	it('Should change language to German when language selector is changed', () => {
		cy.getPageData().then(() => {
			// Change language
			cy.getCy('language-selector').click();
			cy.getCy('option-de-DE').click();

			// Change short date format
			cy.getCy('short-date-format').click();
			cy.getCy('option-yyyy-MM-dd').click();

			// Change long date format
			cy.getCy('long-date-format').click();
			cy.getCy('option-EEEE, MMMM dd, yyyy').click();

			// Change long date format
			cy.getCy('time-format').click();
			cy.getCy('option-pp').click();

			// Change relative dates
			cy.getCy('relative-date').click();

			// Change relative dates
			cy.getCy('ask-download-movie-confirmation').click();
			cy.getCy('ask-download-tvshow-confirmation').click();
			cy.getCy('ask-download-season-confirmation').click();
			cy.getCy('ask-download-episode-confirmation').click();

			cy.awaitSettingsUpdate().then((interception) => {
				const settings = interception.request.body as SettingsModelDTO;
				// Change language
				expect(settings.languageSettings.language).to.equal('de-DE');

				// Change short date format
				expect(settings.dateTimeSettings.shortDateFormat).to.equal('yyyy-MM-dd');

				// Change long date format
				expect(settings.dateTimeSettings.longDateFormat).to.equal('EEEE, MMMM dd, yyyy');

				// Change time format
				expect(settings.dateTimeSettings.timeFormat).to.equal('pp');

				// Change relative dates
				expect(settings.dateTimeSettings.showRelativeDates).to.equal(true);

				// Change confirmation settings
				expect(settings.confirmationSettings.askDownloadMovieConfirmation).to.equal(false);
				expect(settings.confirmationSettings.askDownloadTvShowConfirmation).to.equal(false);
				expect(settings.confirmationSettings.askDownloadSeasonConfirmation).to.equal(false);
				expect(settings.confirmationSettings.askDownloadEpisodeConfirmation).to.equal(false);
			});
		});
	});
});
