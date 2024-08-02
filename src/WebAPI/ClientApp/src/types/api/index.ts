import { Download } from '@api/generated/Download';
import { FolderPath } from '@api/generated/FolderPath';
import { PlexLibrary } from '@api/generated/PlexLibrary';
import { Notification } from '@api/generated/Notification';
import { PlexMedia } from '@api/generated/PlexMedia';
import { Settings } from '@api/generated/Settings';
import { PlexServer } from '@api/generated/PlexServer';
import { PlexServerConnection } from '@api/generated/PlexServerConnection';
import { PlexAccount } from '@api/generated/PlexAccount';

export * from './baseApi';

const downloadApi = new Download();
const folderPathApi = new FolderPath();
const plexLibraryApi = new PlexLibrary();
const notificationApi = new Notification();
const plexMediaApi = new PlexMedia();
const settingsApi = new Settings();
const plexServerApi = new PlexServer();
const plexAccountApi = new PlexAccount();
const plexServerConnectionApi = new PlexServerConnection();

export {
	downloadApi,
	folderPathApi,
	plexLibraryApi,
	notificationApi,
	plexMediaApi,
	settingsApi,
	plexServerApi,
	plexServerConnectionApi,
	plexAccountApi,
};
