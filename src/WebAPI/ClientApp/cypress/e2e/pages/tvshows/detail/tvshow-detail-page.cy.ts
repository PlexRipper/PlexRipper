import { route } from '@fixtures';
import { PlexMediaType } from '@dto';

describe('TV-Show Detail Page', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexServerCount: 1,
			plexTvShowLibraryCount: 1,
			tvShowCount: 1,
			seasonCount: 5,
			episodeCount: 10,
		}).then(({ mediaData }) => {
			const testData = mediaData.find((x) => x.media.some((y) => y.type === PlexMediaType.TvShow));
			cy.wrap(testData).should('not.be.undefined');
			if (!testData) {
				return;
			}
			cy.visit(route(`/tvshows/${testData.libraryId}/details/${testData.media[0].id}`));
		});
	});

	it('Should show selected value when the root select box is clicked', () => {
		cy.getPageData().then(() => {
			cy.getCy('media-list-root-checkbox').click();
			cy.getCy('media-list-root-total-selected-count').should('contain.text', '50');
			cy.getCy('media-overview-bar-download-button').should('be.visible');
		});
	});
});
