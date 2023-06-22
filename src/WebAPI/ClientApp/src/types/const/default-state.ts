import IStoreState from '@interfaces/service/IStoreState';
import IAppConfig from '@class/IAppConfig';

const defaultState: IStoreState = {
	config: {} as IAppConfig,
	pageReady: false,
	accounts: [],
	servers: [],
	serverConnections: [],
	libraries: [],
	mediaUrls: [],
	notifications: [],
	folderPaths: [],
	alerts: [],
	helpIdDialog: '',
	downloadTaskUpdateList: [],
	// Progress Service
	fileMergeProgressList: [],
	inspectServerProgress: [],
	syncServerProgress: [],
	libraryProgress: [],
	serverDownloads: [],
	serverConnectionCheckStatusProgress: [],
	jobStatus: [],
};

export default defaultState;
