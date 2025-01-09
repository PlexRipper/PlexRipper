import { route } from '@fixtures';
import { PlexMediaType } from '@dto';
import { SettingsPaths } from '@api-urls';

describe('Display media collection on the Library detail page', () => {
	it('Should successfully scroll to the bottom when scrolling the page', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 10000,
		})
			.then((data) => {
				// Once the option has changed, the settings are saved
				cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
					statusCode: 200,
				});

				const movieLibrary = data.plexLibraries.find((x) => x.type === PlexMediaType.Movie);
				if (!movieLibrary) {
					throw new Error('Movie library not found');
				}
				// Visit the page
				cy.visit(route(`/movies/${movieLibrary.id}`));

				cy.getCy('change-view-mode-btn').click();
				cy.getCy('view-mode-table-btn').click();

				cy.getCy('media-table-scroll').scrollTo('bottom', { duration: 10000 });

				cy.getCy(`media-table-row-${data.config.movieCount - 1}`)
					.should('exist')
					.and('be.visible');
			});
	});

	it('Should display and click on all the letters in the alphabet navigation when movies are available', function () {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 10000,
		})
			.then((data) => {
				const movieLibrary = data.plexLibraries.find((x) => x.type === PlexMediaType.Movie);
				if (!movieLibrary) {
					throw new Error('Movie library not found');
				}
				// Visit the page
				cy.visit(route(`/movies/${movieLibrary.id}`));

				const movies = data.mediaData.find((x) => x.libraryId === movieLibrary.id)?.media ?? [];
				const sortTitles = movies.map((x) => x.title[0]?.toLowerCase() ?? '#');
				for (const letter of 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.toLowerCase()) {
					const index = sortTitles.indexOf(letter);
					if (index > -1) {
						cy.getCy(`letter-${letter}-alphabet-navigation-btn`).should('be.visible');
						cy.getCy(`letter-${letter}-alphabet-navigation-btn`, { timeout: 10000 }).click();
					}
				}
			});
	});
});
