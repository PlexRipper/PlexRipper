import {
	DownloadTaskDTO,
	FileMergeProgress,
	InspectServerProgressDTO,
	LibraryProgress,
	PlexLibraryDTO,
	ServerConnectionCheckStatusProgressDTO,
	SyncServerProgress,
} from '@dto/mainApi';

export default interface IStoreState {
	libraries: PlexLibraryDTO[];
	downloadTaskUpdateList: DownloadTaskDTO[];
	// Progress Service
	libraryProgress: LibraryProgress[];
	fileMergeProgressList: FileMergeProgress[];
	inspectServerProgress: InspectServerProgressDTO[];
	serverConnectionCheckStatusProgress: ServerConnectionCheckStatusProgressDTO[];
	syncServerProgress: SyncServerProgress[];
}
