import IStoreState from '@interfaces/service/IStoreState';
import IAppConfig from '@class/IAppConfig';

const defaultState: IStoreState = {
	config: {} as IAppConfig,
	pageReady: false,
	libraries: [],
	mediaUrls: [],
	alerts: [],
	helpIdDialog: '',
	downloadTaskUpdateList: [],
	// Progress Service
	fileMergeProgressList: [],
	inspectServerProgress: [],
	syncServerProgress: [],
	libraryProgress: [],
	serverConnectionCheckStatusProgress: [],
};

export default defaultState;
