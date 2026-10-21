import { headers, route } from '@fixtures';
import { generateResultDTO } from '@mock';
import { PlexMediaType, PhotoItemKind, type PhotoAlbumBrowseDTO } from '@dto';

const libraryId = 41;
const serverId = 3;
const albumId = 301;
const album: PhotoAlbumBrowseDTO = {
	id: albumId,
	plexServerId: serverId,
	plexLibraryId: libraryId,
	plexServerName: 'Photo server',
	plexLibraryTitle: 'Photo library',
	title: 'Downloadable album',
	hasThumbnail: false,
	plexApiRatingKey: 301,
	plexApiMetaDataKey: 301,
	assetCount: 1,
	assetBytes: 2048,
};

function setup() {
	cy.intercept({ method: 'GET', pathname: '/api/**' }, (request) => {
		if (request.url.includes('/api/PlexPhoto/')) return;
		throw new Error(`Unmocked API request: ${request.method} ${request.url}`);
	});
	cy.basePageSetup({
		seed: 41,
		plexServerCount: 1,
		plexMovieLibraryCount: 1,
		plexTvShowLibraryCount: 0,
		movieCount: 0,
		tvShowCount: 0,
		firstTimeSetup: false,
		override: {
			plexServer: (items) => items.map((item) => ({ ...item, id: serverId, name: 'Photo server' })),
			plexServerConnections: (items) => items,
			plexLibraries: (items) => items.map((item) => ({ ...item, id: libraryId, type: PlexMediaType.PhotoAlbum, plexServerId: serverId, title: 'Photo library', isEnabled: true, syncedAt: '2026-01-01T00:00:00Z' })),
			plexAccounts: (items) => items.map((item) => ({ ...item, plexServerAccess: [serverId], plexLibraryAccess: [libraryId] })),
			downloadTasks: (items) => items,
			settings: (settings) => ({ ...settings, confirmationSettings: { ...settings.confirmationSettings, askDownloadPhotoAlbumConfirmation: true } }),
		},
	});
	cy.intercept('GET', '**/api/PlexPhoto/albums*', {
		statusCode: 200,
		body: generateResultDTO({ items: [album], page: 1, pageSize: 100, totalCount: 1, totalAssetBytes: 2048 }),
		...headers,
	}).as('photoAlbums');
	cy.intercept({ method: 'GET', pathname: '/api/PlexPhoto/assets' }, {
		statusCode: 200,
		body: generateResultDTO({ items: [], page: 1, pageSize: 100, totalCount: 0, totalBytes: 0 }),
		...headers,
	}).as('photoAssets');
	cy.intercept('POST', '**/api/Download/preview', (request) => {
		request.reply({
			statusCode: 200,
			body: generateResultDTO({
				previews: [{ key: 'album-301', title: album.title, type: PlexMediaType.PhotoAlbum, size: album.assetBytes, qualities: [], children: [] }],
				totalSize: album.assetBytes,
				expanded: { 'album-301': true },
			}),
			...headers,
		});
	}).as('photoPreview');
	cy.intercept('POST', '**/api/Download/create', (request) => {
		request.reply({
			statusCode: 200,
			body: generateResultDTO({ movies: 0, tvShows: 0, seasons: 0, episodes: 0, musicArtists: 0, musicAlbums: 0, musicTracks: 0, photoFolders: 1, photoImages: 1, otherVideos: 0, total: 2 }),
			...headers,
		});
	}).as('photoCreate');
}

describe('Photos download lifecycle', () => {
	it('sends an explicit album selector through preview and create without legacy quality selectors', () => {
		setup();
		cy.visit(route('/photos'));
		cy.wait('@photoAlbums');
		cy.getCy('change-view-mode-btn').filter(':visible').click();
		cy.getCy('view-mode-table-btn').filter(':visible').click();
		cy.getCy('photo-overview-table').contains('tbody tr', album.title).find('[role="checkbox"]').click();
		cy.getCy('media-overview-bar-download-button').click();
		cy.wait('@photoPreview').its('request.body').should((body) => {
			expect(body.downloadMedias).to.have.length(1);
			expect(body.downloadMedias[0]).to.include({ plexLibraryId: libraryId, plexServerId: serverId, type: PlexMediaType.PhotoAlbum, selectionKind: PhotoItemKind.Album });
			expect(body.downloadMedias[0].qualities).to.deep.equal([]);
			expect(body.destinationFolderPathId).to.be.a('number');
		});
		cy.getCy('download-confirmation-submit').should('be.visible').click();
		cy.wait('@photoCreate').its('request.body.downloadMedias[0]').should('include', { type: PlexMediaType.PhotoAlbum, selectionKind: PhotoItemKind.Album });
		cy.getCy('download-confirmation-submit').should('not.exist');
		cy.getCy('change-view-mode-btn').filter(':visible').click();
		cy.getCy('view-mode-poster-btn').filter(':visible').click();
		cy.getCy(`photo-album-${libraryId}-${albumId}`).should('be.visible');
	});
});
