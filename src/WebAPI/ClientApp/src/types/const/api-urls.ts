// region General

export const DEV_APP_URL = 'http://localhost:3000';
export const BASE_URL = 'http://localhost:5000';
export const BASE_API_URL = `${BASE_URL}/api`;

// endregion

// region Relative

export const PLEX_SERVER_RELATIVE_PATH = '/PlexServer';
export const PLEX_SERVER_CONNECTION_RELATIVE_PATH = '/PlexServerConnection';
export const DOWNLOAD_RELATIVE_PATH = '/Download';
export const FOLDER_PATH_RELATIVE_PATH = '/FolderPath';
export const SETTINGS_RELATIVE_PATH = '/Settings';
export const PLEX_LIBRARY_RELATIVE_PATH = '/PlexLibrary';
export const PLEX_ACCOUNT_RELATIVE_PATH = '/PlexAccount';
export const NOTIFICATION_RELATIVE_PATH = '/Notification';
export const HEALTH_RELATIVE_PATH = '/Health';
export const PLEX_MEDIA_RELATIVE_PATH = '/PlexMedia';

// endregion

// region Absolute
export const PLEX_SERVER_API_URL = `${BASE_API_URL}/${PLEX_SERVER_RELATIVE_PATH}`;
export const PLEX_SERVER_CONNECTION_API_URL = `${BASE_API_URL}/${PLEX_SERVER_CONNECTION_RELATIVE_PATH}`;
export const DOWNLOAD_API_URL = `${BASE_API_URL}/${DOWNLOAD_RELATIVE_PATH}`;
export const FOLDER_PATH_API_URL = `${BASE_API_URL}/${FOLDER_PATH_RELATIVE_PATH}`;
export const PLEX_LIBRARY_API_URL = `${BASE_API_URL}/${PLEX_LIBRARY_RELATIVE_PATH}`;
export const PLEX_ACCOUNT_API_URL = `${BASE_API_URL}/${PLEX_ACCOUNT_RELATIVE_PATH}`;
export const SETTINGS_API_URL = `${BASE_API_URL}/${SETTINGS_RELATIVE_PATH}`;
export const NOTIFICATION_API_URL = `${BASE_API_URL}/${NOTIFICATION_RELATIVE_PATH}`;

export const PROGRESS_HUB_URL = `${BASE_URL}/progress`;
export const NOTIFICATIONS_HUB_URL = `${BASE_URL}/notifications`;

// endregion

/**
 * The baseURL for the application. It is assumed that front and back-end run in a docker container on the same URL when in production
 * due to being deployed statically in the wwwroot of the .NET Core back-end. The url is retrieved dynamically as to work with different domains.
 * When in development, the front-end runs on port 3000 and the back-end on port 5000.
 * When in production, the front-end runs on the same port as the back-end, by default on port 7000.
 */
export function getBaseURL(isProduction: boolean) {
	// Are we production testing in a local development environment
	if (isProduction && window.location.origin === DEV_APP_URL) {
		return BASE_URL;
	}

	// Docker production environment
	return isProduction ? window.location.origin : BASE_URL;
}
