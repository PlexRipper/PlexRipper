import { headers, route } from '@fixtures';
import { PhotoAssetKind, PhotoImageRepresentation, PlexMediaType, type PhotoAssetBrowseDTO } from '@dto';
import { generateResultDTO } from '@mock';

const libraryId = 41;
const serverId = 3;
const assets: PhotoAssetBrowseDTO[] = [501, 502].map((id) => ({
	id, plexServerId: serverId, plexLibraryId: libraryId,
	plexServerName: 'Photo server', plexLibraryTitle: 'Photo library', title: id === 501 ? 'Small original' : 'Large original',
	assetKind: PhotoAssetKind.Image, hasThumbnail: false, plexApiRatingKey: id, plexApiMetaDataKey: id,
	originalFilename: `${id}.jpg`, size: id === 501 ? 2048 : 64 * 1024 * 1024 + 1,
	container: 'jpg', width: 6000, height: 4000,
}));
const imageResponse = {
	statusCode: 200,
	body: '<svg xmlns="http://www.w3.org/2000/svg" width="6000" height="4000"><rect width="6000" height="4000" fill="#135"/></svg>',
	headers: { ...headers.headers, 'content-type': 'image/svg+xml' },
};

describe('Photo original preloading', () => {
	it('defers a known oversized original until it is opened without blocking normal preloads', () => {
		// Arrange
		cy.viewport(1280, 800);
		cy.basePageSetup({
			seed: 41, plexServerCount: 1, plexMovieLibraryCount: 1, plexTvShowLibraryCount: 0,
			movieCount: 0, tvShowCount: 0, firstTimeSetup: false,
			override: {
				plexServer: (items) => items.map((item) => ({ ...item, id: serverId, name: 'Photo server' })),
				plexLibraries: (items) => items.map((item) => ({ ...item, id: libraryId, type: PlexMediaType.PhotoAlbum, plexServerId: serverId, title: 'Photo library', isEnabled: true, syncedAt: '2026-01-01T00:00:00Z' })),
				plexAccounts: (items) => items.map((item) => ({ ...item, plexServerAccess: [serverId], plexLibraryAccess: [libraryId] })),
			},
		});
		cy.intercept({ method: 'GET', pathname: '/api/PlexPhoto/assets' }, {
			statusCode: 200, body: generateResultDTO({ items: assets, page: 1, pageSize: 100, totalCount: 2, totalBytes: assets.reduce((sum, asset) => sum + asset.size, 0) }), ...headers,
		}).as('photoAssets');
		cy.intercept({ method: 'GET', pathname: `/api/PlexPhoto/libraries/${libraryId}/assets/501/image` }, imageResponse).as('smallOriginal');
		let largeReads = 0;
		cy.intercept({ method: 'GET', pathname: `/api/PlexPhoto/libraries/${libraryId}/assets/502/image` }, (request) => {
			largeReads += 1;
			expect(request.query.representation).to.equal(PhotoImageRepresentation.Original);
			request.reply(imageResponse);
		}).as('largeOriginal');
		cy.visit(route(`/photos/${libraryId}?view=assets`));
		cy.wait('@photoAssets');
		cy.getCy('change-view-mode-btn').filter(':visible').click();
		cy.getCy('view-mode-poster-btn').filter(':visible').click();
		cy.getCy('photo-asset-41-502').should('be.visible');
		cy.wait('@smallOriginal').then(({ request }) => {
			expect(new URL(request.url).searchParams.get('representation')).to.equal(PhotoImageRepresentation.Original);
		});
		cy.then(() => expect(largeReads, 'no known-oversized background transfer').to.equal(0));

		// Act
		cy.getCy('photo-asset-41-502').click();
		cy.wait('@largeOriginal');

		// Assert
		cy.getCy('photo-viewer-image').should('have.attr', 'alt', 'Large original').and('have.prop', 'naturalWidth', 6000);
		cy.then(() => expect(largeReads, 'one foreground original request').to.equal(1));
	});
});
