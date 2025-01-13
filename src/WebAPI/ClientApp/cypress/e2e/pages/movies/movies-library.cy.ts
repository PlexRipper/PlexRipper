import { route } from '@fixtures';
import { PlexMediaType } from '@dto';

describe('Display media collection on the Library detail page', () => {
	it('Should successfully scroll to the bottom when scrolling the page', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 1000,
		})
			.then(({ mediaData, plexLibraries }) => {
				const movieLibrary = plexLibraries.find((x) => x.type === PlexMediaType.Movie);
				if (!movieLibrary) {
					throw new Error('Movie library not found');
				}
				// Visit the page
				cy.visit(route(`/movies/${movieLibrary.id}`));

				cy.getCy('change-view-mode-btn').click();
				cy.getCy('view-mode-table-btn').click();

				cy.getCy('media-table-scroll').scrollTo('bottom', { duration: 10000 });
				const movieList = mediaData.find((x) => x.libraryId === movieLibrary.id)?.media;
				cy.getCy(`media-table-row-${movieList!.length - 1}`)
					.should('exist')
					.and('be.visible');
			});
	});

	it('Should display and click on all the letters in the alphabet navigation when movies are available', function () {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 1000,
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
						cy.log(`Navigating to letter: ${letter} at index: ${index}`);
						cy.getCy(`letter-${letter}-alphabet-navigation-btn`, { timeout: 10000 }).should('be.visible');
						cy.getCy(`letter-${letter}-alphabet-navigation-btn`, { timeout: 10000 }).click();
						cy.get(`[data-scroll-index="${index}"]`, { timeout: 10000 }).should('be.visible');
					}
				}
			});
	});
});
