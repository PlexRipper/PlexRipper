import { Observable, of } from 'rxjs';
import { getAllDownloads, deleteDownloadTasks, clearDownloadTasks, stopDownloadTasks, downloadMedia } from '@api/plexDownloadApi';
import { finalize, switchMap } from 'rxjs/operators';
import { DownloadMediaDTO, DownloadTaskDTO, PlexServerDTO } from '@dto/mainApi';
import StoreState from '@state/storeState';
import ServerService from '@state/serverService';
import AccountService from '@service/accountService';
import { BaseService } from '@state/baseService';

export class DownloadService extends BaseService {
	public constructor() {
		super((state: StoreState) => {
			return {
				downloads: state.downloads,
			} as StoreState;
		});

		AccountService.getActiveAccount()
			.pipe(switchMap(() => getAllDownloads()))
			.subscribe((downloads: DownloadTaskDTO[]) => {
				if (downloads) {
					this.setState({ downloads }, 'Initial DownloadTask Data');
				}
			});
	}

	public getDownloadList(): Observable<DownloadTaskDTO[]> {
		return this.stateChanged.pipe(switchMap((state: StoreState) => of(state?.downloads ?? [])));
	}

	/**
	 * returns the downloadTasks nested in PlexServerDTO -> PlexLibraryDTO -> DownloadTaskDTO[]
	 */
	public getDownloadListInServers(): Observable<PlexServerDTO[]> {
		return ServerService.getServers().pipe(
			switchMap(() => this.getDownloadList()),
			switchMap((downloads) => {
				const servers: PlexServerDTO[] = [];
				downloads.forEach((download) => {
					// Check if Server has already been added
					let server = servers.find((x) => x.id === download.plexServerId);
					if (!server) {
						server = ServerService.servers.find((x) => x.id === download.plexServerId);
						if (server) {
							servers.push(server);
						}
					}

					if (server) {
						const libraryIndex = server.plexLibraries.findIndex((x) => x.id === download.plexLibraryId);
						if (libraryIndex > -1) {
							server.plexLibraries[libraryIndex].downloadTasks.push(download);
						}
					}
				});
				return of(servers);
			}),
		);
	}

	/**
	 * Fetch the download list and signal to the observers that it is done.
	 */
	public fetchDownloadList(): Observable<DownloadTaskDTO[]> {
		getAllDownloads().subscribe((downloads) => this.setState({ downloads }));
		return this.getDownloadList();
	}

	public downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): void {
		downloadMedia(downloadMediaCommand)
			.pipe(finalize(() => this.fetchDownloadList()))
			.subscribe();
	}

	// region Commands

	public deleteDownloadTasks(downloadTaskIds: number[]): void {
		this.removeDownloadTasks(downloadTaskIds);
		deleteDownloadTasks(downloadTaskIds)
			.pipe(finalize(() => this.fetchDownloadList()))
			.subscribe();
	}

	public clearDownloadTasks(downloadTaskIds: number[]): void {
		this.removeDownloadTasks(downloadTaskIds);
		clearDownloadTasks(downloadTaskIds)
			.pipe(finalize(() => this.fetchDownloadList()))
			.subscribe();
	}

	public stopDownloadTasks(downloadTaskIds: number[]): void {
		stopDownloadTasks(downloadTaskIds)
			.pipe(finalize(() => this.fetchDownloadList()))
			.subscribe();
	}

	// endregion

	private removeDownloadTasks(downloadTaskIds: number[]): void {
		const downloads = this.getState().downloads.filter((x) => downloadTaskIds.some((y) => y !== x.id));
		this.setState({ downloads });
	}
}

const downloadService = new DownloadService();
export default downloadService;
