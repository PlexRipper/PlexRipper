import { route } from '@fixtures/baseE2E';
import { type LibraryProgress, MessageTypes, PlexMediaType } from '@dto';
import { PlexLibraryPaths } from '@api/api-paths';

describe('Test the refreshing of a PlexLibrary', () => {
	beforeEach(() => {
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
				return cy.wrap({
					...data,
					movieLibrary,
				});
			})
			.as('movieLibrary');
	});

	it('Should display refreshing of the PlexLibrary when sending the refreshing command', () => {
		cy.getPageData().then((data) => {
			const movieLibrary = data.plexLibraries.find((x) => x.type === PlexMediaType.Movie)!;

			cy.intercept('POST', PlexLibraryPaths.refreshLibraryMediaEndpoint(movieLibrary.id), (req) => {
				expect(req.body).to.eql({ plexLibraryId: movieLibrary?.id ?? -1 });
				req.reply({
					statusCode: 200,
				});
			}).as('refreshLibrary');
			cy.getCy(`media-overview-refresh-library-btn`).click();

			for (let i = 0; i < 5; i++) {
				cy.wait(500).hubPublish('progress', MessageTypes.LibraryProgress, {
					id: movieLibrary?.id ?? -1,
					percentage: i * 25,
					received: i * 25,
					total: 100,
					timeStamp: new Date().toISOString(),
					isRefreshing: true,
					isComplete: i === 4,
				} as LibraryProgress);
				cy.getCy('refresh-library-container').should('be.visible');
			}
			cy.getCy('refresh-library-container').should('not.be.exist');
		});
	});
});
