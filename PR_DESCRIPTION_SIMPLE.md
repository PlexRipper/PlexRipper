Adds Torznab API integration to make PlexRipper function as an indexer for Sonarr/Radarr.

This feature lets you search and download from your big Plex server to your small server using Sonarr/Radarr. When you search for new content, if the big Plex server has it, your trackers would be skipped and PlexRipper would download the media instead.

## What's Added

- Torznab-compatible API endpoints
- Multi-server search (searches all your Plex servers by default)
- Server filtering options in settings
- API key authentication
- Download integration with existing PlexRipper system

## How to Use

1. Enable Torznab in PlexRipper settings
2. Note your API key (or set your own)
3. Add a Torznab indexer in Sonarr/Radarr:
   - URL: http://[your-plexripper-address]/torznab
   - API Key: from settings
   - Categories: 2000, 5000 (movies and TV)

Now when Sonarr/Radarr searches for content, PlexRipper will check your Plex servers first.

## Why It's Useful

- Save bandwidth - don't download stuff you already have on another server
- Works with your existing automation - no workflow changes needed
- Makes managing multiple Plex servers easier
