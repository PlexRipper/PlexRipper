export enum StoreNames {
	AccountStore = 'AccountStore',
	AlertStore = 'AlertStore',
	AuthenticationStore = 'AuthenticationStore',
	BackgroundJobsStore = 'BackgroundJobsStore',
	DialogStore = 'DialogStore',
	DownloadStore = 'DownloadStore',
	FolderPathStore = 'FolderPathStore',
	GlobalStore = 'GlobalStore',
	HelpStore = 'HelpStore',
	IntegrationStore = 'IntegrationStore',
	LibraryStore = 'LibraryStore',
	LocalizationStore = 'LocalizationStore',
	MediaStore = 'MediaStore',
	NotificationsStore = 'NotificationsStore',
	ServerConnectionStore = 'ServerConnection',
	ServerStore = 'ServerStore',
	SettingsStore = 'SettingsStore',
	SignalrStore = 'SignalrStore',
	PageSetup = 'PageSetup',
	AccountDialogStore = 'AccountDialogStore',
	MediaOverviewStore = 'MediaOverviewStore',
}

export interface ISetupResult {
	name: StoreNames;
	isSuccess: boolean;
}
