## [0.10.0-RC1]

WORK IN PROGRESS

### Added


### Changed
 - Each PlexServer now has multiple connections available which can be chosen individually, this should solve any proxy and connection issues with servers.
 - Each PlexServer can now have a preferred connection that can be set in the server settings 
 - Changed the infrastructure for the download and fileMerge process to be hosted in the Quartz Background services
 - Big re-organization of the PlexAccount setup process

### Fixed
 - Fixed the two-factor authentication not working due to the Plex error not being passed on from the http client
 - Fixed the front-end page data not refreshing when deleting an PlexAccount

## [0.9.1]

### Added
 - Added a setup question on the home page instead of forcefully redirecting to the setup page when the setup hasn't been yet skipped or completed
### Changed
 - Made the updating of settings in the front-end a true observable to make it chain-able and await-able to only do stuff after it has updated the settings
### Fixed
 - Fixed the setup loop that happened for some users where they got redirected back to the setup screen after finishing or skipping by removing the redirect
 - Fixed the downloadSegments setting not updating correctly when changed

## [0.9.0]

This has been a major refactoring with many unit and integration tests added to ensure stability.
This means you will have to empty the config folder before installing to ensure proper workings.

Note: This, by a long shot, doesn't encompasses all the changes and fixes that have been made in this version due to the large scope of the changes.

### Added
 - Per server configurable download speed limit (See server settings > Server Configuration)
 - Per server and library configurable folder destination where media downloaded will automatically be moved too.
 - German UI language (Thanks to [Padso4tw](https://github.com/padso4tw)!)
 - Jest and Cypress testing infrastructure
 - Added a new server command in the server settings to inspect a server connection and attempt to fix it.
 - Added a loading screen with a rotating logo

### Changed
 - Removed batch commands from the download page, these were not working and overcomplicated things too much.
 - Migrated project to .NET 6, which brings many performance improvements
 - Added a loading icon to the button when checking the server status in the server configuration
 - The server command "Re-sync Library media" now displays a loading animation.
 - Replaced every button with a more performant and consistent button construction
 - Thumbnails displayed for movies and tvShows on the library pages in poster mode are now displayed from cache when navigating around PlexRipper. 
 - Rewrote the code for the entire download process to be more stable and allow for future features
 - Rewrote the user settings modules (PlexRipperConfig.json) which is now easy to extend and more resilient.
 - Many performance improvements!

### Fixed
 - Fixed the setup of Plex accounts in PlexRipper which was failing due to a login format change on the side of Plex
 - Fixed the slow downloads by increasing the download buffer, might make it configurable based on the hardware PlexRipper is run on. (Thanks to [BakasuraRCE](https://github.com/BakasuraRCE)!)
 - Fixed the opening of the server settings not defaulting back to its first tab
 - Fixed the download progress not updating after a while due to SignalR disconnects
 - Fixed the retrieval of the ServerStatus not working when a timeout happens.
 - Fixed the notifications not always being shown and updated correctly when an error happens.
 - Fixed the download confirmation window not hiding after clicking confirm #122
 - Fixed the page background effect breaking when the browser does not support WebGL. It will now display a still image of the background.

## [0.8.7]

### Added

- Added two-factor authentication compatibility, meaning PlexRipper now works with two-factor authentication protected Plex accounts.
- Added the awesome French translation from @starnakin, thank you so much!
- Added a progress window showing the individual servers being connected to when setting up an PlexAccount.
- Added a warning that deleting an plexAccount might take a long time due to the amount of data which has to be deleted.
- Added a thank you for the awesome contribution from Starnakin to the README.
- Added migration check for adding ClientId to already created Plex accounts, this avoids users having to re-setup their accounts in PlexRipper.

### Changed

- When confirming an action in the confirmation dialog, a loading spinner will now show.
- When a translation is missing, it will now show the English variant.
- When no downloads have been selected in the download page, the "Clear Completed" is now disabled.
- Updated to the new PlexAPI SignIn process
- Improved the PlexApi HttpClient to return errors given by Plex.

### Fixed

- Fixed the "New device connected" spam that Plex server owners would get due to a randomly generated ClientId's being used per request by PlexRipper. This is now unique and consistent for every PlexAccount.
- Fixed the delete button missing from the Plex account update window.
- Fixed the confirmation window prevented from being closed.
- Fixed the menu titles now being translatable.
- Fixed the DateTime settings not being translatable.
- Fixed the browser client not updating its store after resetting the database.
- Fixed the error window having a very wide window due to unwrapped text.
- Fixed the TvShows libraries getting stuck in a infinite loading loop when viewed, TV show libraries can now be viewed again.

## [0.8.6]

### Added

- Added an improved Notification sidebar
- Added all text as translations keys to the language file to ensure everything can get translated.
- Added Language switcher under Settings => UI, only supported English and the to be translated French.

### Changed

- Notifications can now be cleared with a click of a button
- Fixed the lang folder link in the README
- Added the French language file and language support (Still needs translations).
- Added the language option to the config file
- Rewritten how settings are saved, they should now work much better
- Added feedback section to the ReadMe
- Ensured the config/settings file is now better protected against corruption and invalid values

### Fixed

- Fixed the Letter navigation in Poster view not scrolling the page.
- Fixed the setup page not shown automatically on a new install.
- Fixed the percentage in download speed having too many decimals, namely in the Downloads page.
- Fixed where empty help buttons would show up next to options.
- Fixed the checkboxes in settings not working.
- Fixed the misalignment of the checkboxes in the settings page.
- Fixed an issue where statically generating the WebUI in development would return the wrong api url.
- Fixed an error when trying to add a Movie or TvShow destination folder under Settings => Paths, should now work correctly
- Fixed the "no downloads" message not always showing correctly when there are no downloads in progress.
- Fixed the setup page not showing the correct background and throwing an error when that happens
- Fixed the skip button during the setup wizard not appearing