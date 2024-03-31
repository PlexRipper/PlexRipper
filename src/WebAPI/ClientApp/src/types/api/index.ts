import Log from 'consola';
import Axios, { AxiosRequestConfig } from 'axios';
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

const axiosConfig: AxiosRequestConfig = {};
Log.info('Setting Axios Config:', axiosConfig);

const downloadApi = new Download({ ...axiosConfig });
const folderPathApi = new FolderPath({ ...axiosConfig });
const plexLibraryApi = new PlexLibrary({ ...axiosConfig });
const notificationApi = new Notification({ ...axiosConfig });
const plexMediaApi = new PlexMedia({ ...axiosConfig });
const settingsApi = new Settings({ ...axiosConfig });
const plexServerApi = new PlexServer({ ...axiosConfig });

const plexAccountApi = new PlexAccount({ ...axiosConfig });
const plexServerConnectionApi = new PlexServerConnection({ ...axiosConfig });

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
