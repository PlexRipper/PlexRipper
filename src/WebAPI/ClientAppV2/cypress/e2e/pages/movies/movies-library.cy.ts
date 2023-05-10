import { get } from '@vueuse/core';
import { apiRoute, route } from '@fixtures/baseE2E';
import { PLEX_LIBRARY_RELATIVE_PATH, PLEX_MEDIA_RELATIVE_PATH } from '@api-urls';
import { generateResultDTO } from '@mock';
import { PlexLibraryDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import { generatePlexMedias } from '@factories';

describe('Display media collection on the Library detail page', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 2,
			plexLibraryCount: 3,
		})
			.then((pageData) => {
				// Generate a bunch of movies
				const movieCount = 1000;
				const plexLibrary: PlexLibraryDTO = {
					...pageData.plexLibraries[0],
					type: PlexMediaType.Movie,
					count: movieCount,
					mediaSize: 10000,
				};
				const movies: PlexMediaSlimDTO[] = generatePlexMedias({
					plexLibraryId: plexLibrary.id,
					plexServerId: plexLibrary.plexServerId,
					type: PlexMediaType.Movie,
					config: {
						movieCount,
					},
				});
				// Intercept the Library detail call
				cy.intercept('GET', apiRoute(PLEX_LIBRARY_RELATIVE_PATH + `/${plexLibrary.id}`), {
					statusCode: 200,
					body: generateResultDTO(plexLibrary),
				});

				// Intercept the Library media call
				cy.intercept('GET', apiRoute(PLEX_MEDIA_RELATIVE_PATH + `/library/${plexLibrary.id}`, '?page=*&size=*'), {
					statusCode: 200,
					body: generateResultDTO(movies),
				});
				// Visit the page
				cy.visit(route(`/movies/${pageData.plexLibraries[0].id}`));

				return cy.wrap({
					plexLibrary,
					movies,
				});
			})
			.as('movieData');
	});

	xit('Should successfully scroll to the bottom when scrolling the page', () => {
		cy.getCy('media-table-scroll').scrollTo('bottom', { duration: 10000 });

		cy.getCy(`media-table-row-${1000 - 1}`)
			.should('exist')
			.and('be.visible');
	});

	it('Should display and click on all the letters in the alphabet navigation when movies are available', function () {
		cy.get('@movieData').then((movieData) => {
			cy.wait(1000);
			const movies = (movieData as any).movies as PlexMediaSlimDTO[];
			const sortTitles = get(movies).map((x) => x.sortTitle[0].toLowerCase());
			for (const letter of 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.toLowerCase()) {
				const index = sortTitles.indexOf(letter);
				if (index > -1) {
					cy.getCy(`letter-${letter}-alphabet-navigation-btn`).should('be.visible');
					cy.getCy(`letter-${letter}-alphabet-navigation-btn`, { timeout: 10000 }).click();
				}
			}
			cy.getCy(`media-table-row-${1000 - 1}`)
				.should('exist')
				.and('be.visible');
		});
	});
});
