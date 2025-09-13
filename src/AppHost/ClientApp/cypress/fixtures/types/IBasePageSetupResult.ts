import type {
	DownloadTaskDTO, FolderPathDTO,
	PlexAccountDTO,
	PlexLibraryDTO, PlexMediaSlimDTO,
	PlexServerConnectionDTO,
	PlexServerDTO,
	ServerDownloadProgressDTO, SettingsModelDTO,
} from '@dto';
import type { MockConfig } from '@mock';

export interface IBasePageSetupResult {
	plexServers: PlexServerDTO[];
	plexServerConnections: PlexServerConnectionDTO[];
	plexLibraries: PlexLibraryDTO[];
	plexAccounts: PlexAccountDTO[];
	serverDownloadProgress: ServerDownloadProgressDTO[];
	detailDownloadTasks: DownloadTaskDTO[];
	mediaData: {
		libraryId: number;
		media: PlexMediaSlimDTO[];
	}[];
	folderPaths: FolderPathDTO[];
	settings: SettingsModelDTO;
	config: MockConfig;
}
