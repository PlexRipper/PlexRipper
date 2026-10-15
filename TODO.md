# Deferred migration fixes

- [ ] Preserve existing genre data in `20261014090024_TEMP`: migrate `PlexGenres` and `PlexLibraryGenres` to their video equivalents without losing IDs, genre rows, library associations, or movie/TV genre joins. Make both upgrade and rollback data-preserving, and verify an upgrade from the preceding schema as well as a fresh database.
- [ ] Restore the deleted folder-path media-type backfills in the replacement migration: normalize `Music` to `MusicArtist` and `Photos` to `PhotoAlbum` in `Up`, with the inverse transformations in `Down`. Verify existing folder paths remain readable after upgrading.
