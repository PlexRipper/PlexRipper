## [0.4.3] - 2020-12-28

### Fixed

-   Possible build fix for IHttpClientFactory not being referenced.

## [0.4.2] - 2020-12-28

### Changed

-   Enabled migrations for the database.
-   Logging will now be written to file in `/config/logs`.
-   Created a static class `FileSystemPaths` for OS paths.

## [0.4.1] - 2020-12-28

### Added

-   Added Polly package to start integrating resilience

### Changed

-   Changed download commands in the DownloadManager to async due to the async dependency of ClearWorkers.
-   Refactored the dependency chain between DownloadManager, DownloadClient and DownloadWorker to be instantiated by Autofac
-   HttpClients are now instantiated through IHttpClientFactory

### Fixed

-   Fixed the LibrarySize not being shown in the front-end …
-   Fixed the DownloadTask creating invalid path if the media title contained invalid characters.

## [0.4.0] - 2020-12-28

### Added

-   Downloads will now be temporally stored in a sub-folder based on media type.
-   Multiple selected downloads in table view mode can now be downloaded in one go, making downloading multiple tvShows or movies a lot easier.

### Changed

-   Lowered the maximal parallel api requests to refresh a tvShow library from 10 to 5 in order to not be blocked by the server.
-   Added a sanitization function when creating download paths to prevent errors on invalid folder names.
-   Major refactor on the download queue handling and fixed several bugs, greatly improving performance and error handling.
-   Refactored the Stop and Clear download task commands from front to back.
-   The footer of a downloadTable in the front-end is now hidden.
-   The DownloadBar commands are now disabled when there is no selection.
-   Cleaned-up the MediaOverviewComponent and VerticalButtonComponent.
-   Fixed the width of the vertical buttons in the MediaOverviewBar such that they are equal width and distance now.
-   The confirmation dialog with an overview of what will be downloaded is now much more efficient and works with whatever is selected.

### Fixed

-   Fixed the download client not deleting downloadTasks from the front-end.
-   Fixed the destination folder naming being wrong with tvShows without seasons, such as Anime.
-   Fixed the downloadCreationProgress update to the front-end.
-   Fixed the spacing that was in download speed, from "45Mb /s" to "45Mb/s".
-   The download confirmation dialog now has the correct scrollbar styling.

### Removed

-   Removed the singular delete function to delete a downloadTask.
-   Deleted the MovieTable and TvShowTable components as they are obsolete.

## [0.3.0] - 2020-12-23

### Added

-   Added media icons to the download confirmation dialog.
-   Tv shows now show their media size in the MediaTableView.

### Changed

-   Cleaned-up the default-layout code in the front-end.
-   Moved the files related to path retrieval of the host system to the FileSystem project.

### Fixed

-   Fixed the setup page not being scroll-able when viewing with limited viewport height.
-   Fixed when going to a music library that nothing would happen, now a page will show that it is currently not supported.
-   Fixed the UI for the path settings and DirectoryBrowser

## [0.2.0] - 2020-12-22

### Added

-   Added CHANGELOG.md file
-   Added PlexRipper version to the front-end in the home button.
-   Added the FileSize.vue component and moved the PrettyBytes filter there.
-   The total media size of a Plex library is now shown in the media bar on the library page.

### Changed

-   Changed the dockerFile path for tvShows from "/series" to "/tvshows"
-   Movies which are downloaded now include the year of release in their folder name.
-   Refactored the config pipeline on front-end startup to work in both development and production.

### Fixed

-   Fixed the CORS issue for SignalR due to missing "AllowCredentials".
-   Fixed a bug in the refreshing of PlexLibraries which have the TvShow type.
    It would not wait for the result of all media, leading to wrong return values

### Removed

-   Removed the CorsMiddleware fix in the back-end as it was not needed anymore

## [0.1.1] - 2020-12-18

### Changed

-   Updated packages front-end packages.
-   Setup build pipeline in Github Actions and in DockerHub.

## [0.1.0] - 2020-12-18

-   Initial release of PlexRipper
