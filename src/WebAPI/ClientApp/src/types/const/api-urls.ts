// region General

export const BASE_URL = 'http://localhost:5000/';
export const BASE_API_URL = `${BASE_URL}/api`;

// endregion

// region Relative

export const PLEX_SERVER_PATH = `/PlexServer`;
export const DOWNLOAD_PATH = `/Download`;
export const FOLDER_PATH_PATH = `/FolderPath`;
export const SETTINGS_RELATIVE_PATH = `/Settings`;
export const PLEX_LIBRARY_RELATIVE_PATH = `/PlexLibrary`;
export const PLEX_ACCOUNT_RELATIVE_PATH = `/PlexAccount`;
export const NOTIFICATION_RELATIVE_PATH = `/Notification`;

// endregion

// region Absolute
export const PLEX_SERVER_API_URL = `${BASE_API_URL}/${PLEX_SERVER_PATH}`;
export const DOWNLOAD_API_URL = `${BASE_API_URL}/${DOWNLOAD_PATH}`;
export const FOLDER_PATH_API_URL = `${BASE_API_URL}/${FOLDER_PATH_PATH}`;
export const PLEX_LIBRARY_API_URL = `${BASE_API_URL}/${PLEX_LIBRARY_RELATIVE_PATH}`;
export const PLEX_ACCOUNT_API_URL = `${BASE_API_URL}/${PLEX_ACCOUNT_RELATIVE_PATH}`;
export const SETTINGS_API_URL = `${BASE_API_URL}/${SETTINGS_RELATIVE_PATH}`;
export const NOTIFICATION_API_URL = `${BASE_API_URL}/${NOTIFICATION_RELATIVE_PATH}`;

export const PROGRESS_HUB_URL = `${BASE_URL}/progress`;
export const NOTIFICATIONS_HUB_URL = `${BASE_URL}/notifications`;
// endregion
