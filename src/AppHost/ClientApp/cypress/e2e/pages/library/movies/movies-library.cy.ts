import { route } from '@fixtures';
import { PlexMediaType } from '@dto';
import { generatePlexMediaStatisticsDTO, generateResultDTO } from '@mock';

describe('Display media collection on the Library detail page', () => {
	it('Should preserve query totals and keep search cleared after reloading', () => {
		// Arrange
		cy.basePageSetup({
			seed: 47019,
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 250,
			isLoggedIn: true,
		}).then(({ mediaData, plexLibraries }) => {
			const movieLibrary = plexLibraries.find((library) => library.type === PlexMediaType.Movie)!;
			const movies = mediaData.find((library) => library.libraryId === movieLibrary.id)!.media;
			const firstSearchMovie = { ...movies[0]!, title: 'First query result', sortIndex: 1 };
			const currentSearchMovie = { ...movies[1]!, title: 'Current query result', sortIndex: 1 };
			cy.intercept('GET', '**/api/PlexMedia*', (request) => {
				if (Number(request.query.plexLibraryId) !== movieLibrary.id) return;
				const query = String(request.query.q ?? '');
				if (query === 'first-query' || query === 'current-query') {
					const movie = query === 'first-query' ? firstSearchMovie : currentSearchMovie;
					const statistics = {
						...generatePlexMediaStatisticsDTO([movie]),
						queryHash: query,
						pageSize: 100,
						totalCount: 1,
					};
					request.alias = query;
					request.reply({ statusCode: 200, body: generateResultDTO(statistics) });
					return;
				}
				request.alias = `movie-page-${String(request.query.page)}`;
			});

			// Act
			cy.visit(route(`/movies/${movieLibrary.id}`));
			cy.wait('@movie-page-1');
			cy.getCy('change-view-mode-btn').filter(':visible').first().click();
			cy.getCy('view-mode-table-btn').click();
			cy.get('#media-table-scroll').should('be.visible').scrollTo('center');
			cy.wait('@movie-page-2');
			cy.get('#media-table-scroll').scrollTo('bottom');
			cy.wait('@movie-page-3').then(({ response }) => {
				expect(response?.body.value.mediaCount).to.equal(250);
				expect(response?.body.value.mediaList).to.have.length(50);
			});

			// Assert
			cy.getCy('media-table-row-249').should('be.visible');
			cy.contains('button', 'Selection').filter(':visible').first().click();
			cy.contains('Set the index range to select the media (#1 - #250)').should('be.visible');
			cy.contains('button', 'Close').filter(':visible').last().click();
			cy.get('input[aria-label="Search media"]').filter(':visible').first().type('first-query');
			cy.wait('@first-query');
			cy.getCy('media-table').should('contain.text', firstSearchMovie.title);
			cy.get('input[aria-label="Search media"]').filter(':visible').first().clear().type('current-query');
			cy.wait('@current-query');
			cy.getCy('media-table')
				.should('contain.text', currentSearchMovie.title)
				.and('not.contain.text', firstSearchMovie.title);

			cy.location('search').should('include', 'q=current-query');
			cy.get('button[aria-label="Clear media search"]').filter(':visible').first().click();
			cy.wait('@movie-page-1').then(({ request, response }) => {
				expect(request.query.q ?? '').to.equal('');
				expect(response?.body.value.totalCount).to.equal(250);
			});
			cy.location('search').should('not.contain', 'q=');
			cy.get('input[aria-label="Search media"]').filter(':visible').first().should('have.value', '');
			cy.getCy('media-table').should('not.contain.text', currentSearchMovie.title);

			cy.reload();
			cy.wait('@movie-page-1').then(({ request }) => {
				expect(request.query.q ?? '').to.equal('');
			});
			cy.location('search').should('not.contain', 'q=');
			cy.get('input[aria-label="Search media"]').filter(':visible').first().should('have.value', '');
		});
	});

	it('Should successfully scroll to the bottom when scrolling the page', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 1000,
			isLoggedIn: true,
		})
			.then(({ mediaData, plexLibraries }) => {
				const movieLibrary = plexLibraries.find((x) => x.type === PlexMediaType.Movie);
				if (!movieLibrary) {
					throw new Error('Movie library not found');
				}
				const movieList = mediaData.find((x) => x.libraryId === movieLibrary.id)?.media ?? [];
				const lastPage = Math.ceil(movieList.length / 100);
				cy.intercept('GET', '**/api/PlexMedia*', (req) => {
					if (Number(req.query.plexLibraryId) === movieLibrary.id && Number(req.query.page) === lastPage) {
						req.alias = 'lastMoviePage';
					}
				});

				// Visit the page
				cy.visit(route(`/movies/${movieLibrary.id}`));
				cy.url({ timeout: 20000 }).should('include', `/movies/${movieLibrary.id}`);

				cy.get('[data-cy="poster-table"] #poster-table', { timeout: 20000 })
					.should('exist')
					.scrollTo('bottom', { duration: 10000 });
				cy.wait('@lastMoviePage', { timeout: 20000 });
				cy.get(`[data-scroll-index="${movieList.length - 1}"]`, { timeout: 20000 })
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
			isLoggedIn: true,
		})
			.then((data) => {
				const movieLibrary = data.plexLibraries.find((x) => x.type === PlexMediaType.Movie);
				if (!movieLibrary) {
					throw new Error('Movie library not found');
				}
				// Visit the page
				cy.visit(route(`/movies/${movieLibrary.id}`));
				cy.url({ timeout: 20000 }).should('include', `/movies/${movieLibrary.id}`);

				cy.get('[data-cy="poster-table"]', { timeout: 20000 }).should('exist');

				const movies = data.mediaData.find((x) => x.libraryId === movieLibrary.id)?.media ?? [];
				const sortTitles = movies.map((x) => x.title[0]?.toLowerCase() ?? '#');
				cy.get('[data-cy$="alphabet-navigation-btn"]').each(($btn) => {
					const dataCy = $btn.attr('data-cy') ?? '';
					const letter = dataCy.replace('letter-', '').replace('-alphabet-navigation-btn', '');
					const index = sortTitles.indexOf(letter);
					if (index > -1) {
						cy.log(`Navigating to letter: ${letter} at index: ${index}`);
						cy.wrap($btn).should('be.visible').click();
						cy.get(`[data-scroll-index="${index}"]`, { timeout: 10000 }).should('be.visible');
					}
				});
			});
	});
});
