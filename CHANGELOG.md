## [0.6.3] - 2021-01-24
### Fixed
-   Fixed the downloadPreview of the media not shown correctly based on the selection.
-   Fixed the downloadTaskCreation progressbar freezing on 0% due to no updates.

## [0.6.2] - 2021-01-24
### Fixed
 - Fixed the movie poster view not being able to download.

## [0.6.1] - 2021-01-23
### Changed
-   Moved several properties in the Downloadtask entity into it's metadata field.
### Fixed
-   Fixed the downloadProcess not working due to major refactoring
-   Fixed some warnings int he Controllers about fields in the baseController being overwritten


## [0.6.0] - 2021-01-21
### Added
-   Added an Alphabet navigation to the tableView and posterView in the library navigation.

### Changed
-   The download size is now shown in the downloadConfirmation dialog.
-   The downloadButton will show to be loading in the tableview when a tvShow has not been retrieved yet.
-   Polished the user interface of the downloadConfirmation window.
-   PlexMedia derived entities now have a reference to the owning plexServer, this saves the trouble of retrieving the PlexLibrary first to get the server.

### Fixed
-   Fixed the downloadButton being too small in tableMode when navigating a library.
-   Fixed the movie fileSize not being shown correctly in tableViewMode.
-   Fixed the selecting of tableRows in the tableview when viewing a library, this was giving incorrect results back.
-   Fixed the movie media rows in the tableView wrongly retrieving season/episode data.
-   Fixed the missing posters showing as if they were still loading, now the will show a default empty image.
-   Fixed the unneeded health-ping to the back-end, this is now disabled.
-   Fixed back-button and close-button of the media details overview.

### Optimized
-   Removed unneeded data from PlexMedia entity that was stored in the database but never used.
-   Optimized the image paths of PlexMedia entity, now they are generated instead of stored. Reduces database storage of image paths by 99%.
-   Optimized the mapping of PlexMedia data coming from the external PlexServers.
-   Optimized the retrieval of thumbnail and banner functionality.
-   Refactored the generating of the downloadPreview shown in the downloadConfirmation dialog.
-   Changed the requesting of thumbnails to a GET request.
-   Removed unneeded values from the PlexMedia DTO's send to the front-end.
-   Improved the mediaDetailScreen.
-   ThumbnailUrl results are now shared and stored in the Store.
-   The mediaTable now returns downloadCommands based on the type, and combines an entire season worth of episodes, into just the seasonId to be send.


## [0.5.0] - 2021-01-18
### Added
-   Added a MediaController to request individual mediaData.
-   Added more documentation in PlexLibrary.

### Changed
-   Refactored the PlexApiClient and integrated Polly to have a more robust ApiClient.
-   Refactored the ResultExtensions.Logging and added LogDebug and LogInformation to results.
-   Added universal string <-> int conversion in AutoMapper.
-   Refactored the RatingKey, Key etc in media, now it's just an int Key and removed the other non-sense.
-   PlexTvShowSeason and PlexTvShowEpisoide now have a ParentKey, this is used to connect everything together when refreshing the library.
-   Refactored the refreshing of a TvShow library, this is now way faster and does away with the individual season/episode requests. Now it only requires 3 API requests total and doesnt look like a DDOS  when refreshing a library.
-   Major performance improvements for showing the library contents in the front-end.
-   Requesting of all episodes now happens in batches as not to overload the server.
-   Moved certain PlexLibrary database fields into a json MetaData field and added ways to only calculate the metadata fields once in order to increase performance.
-   Refreshing a library now doesn't return an updated plexLibrary anymore, in certain cases this would be done multiple times and wasting performance.
-   Added a system to return only top-level media when navigating a Plex library in the front end, when a tvShow is expanded, only then will the full data be requested from the back-end. This was done to handle huge libraries which would slow down performance considerably.

### Fixed
-   Fixed the refreshProgressbar in the mediaoverview hanging when failing to refresh a library.
-   Fixed the episodes not downloading due to a missing TvShowTitle when creating downloadTasks.
-   Fixed the default value for UI => Date And Time => TimeFormat not being correct.
-   Fixed the updating of a PlexAccount which was always returning an error.
-   Fixed the false "Could not be displayed" error when the library is still loading.


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

-   Fixed the LibrarySize not being shown in the front-end
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

-   Initial pre-release of PlexRipper
