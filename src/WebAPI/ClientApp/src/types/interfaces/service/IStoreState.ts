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
	// Progress Service
}
