import Log from 'consola';
import { Observable, of } from 'rxjs';
import { getAllDownloads } from '@api/plexDownloadApi';
import { switchMap } from 'rxjs/operators';
import { DownloadTaskDTO, PlexServerDTO } from '@dto/mainApi';
import { ObservableStore } from '@codewithdan/observable-store';
import StoreState from '@state/storeState';
import ServerService from '@state/serverService';
import AccountService from '@service/accountService';

export class DownloadService extends ObservableStore<StoreState> {
	public constructor() {
		super({
			stateSliceSelector: (state: StoreState) => {
				return {
					downloads: state.downloads,
				} as StoreState;
			},
		});

		AccountService.getActiveAccount()
			.pipe(switchMap(() => getAllDownloads()))
			.subscribe((downloads: DownloadTaskDTO[]) => {
				if (downloads) {
					this.setState({ downloads }, 'Initial DownloadTask Data');
					Log.warn('history', this.stateHistory);
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
}

const downloadService = new DownloadService();
export default downloadService;
