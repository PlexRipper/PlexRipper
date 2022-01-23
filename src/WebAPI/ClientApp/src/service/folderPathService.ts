import { Observable } from 'rxjs';
import { FolderPathDTO } from '@dto/mainApi';
import { finalize, map, take } from 'rxjs/operators';
import { BaseService, GlobalService } from '@service';
import { getFolderPaths, createFolderPath, updateFolderPath, deleteFolderPath } from '@api/pathApi';
import IStoreState from '@interfaces/service/IStoreState';
import { Context } from '@nuxt/types';
import Log from 'consola';

export class FolderPathService extends BaseService {
	public constructor() {
		super({
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					folderPaths: state.folderPaths,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		GlobalService.getAxiosReady()
			.pipe(finalize(() => this.fetchFolderPaths()))
			.subscribe();
	}

	public fetchFolderPaths(): void {
		getFolderPaths()
			.pipe(take(1))
			.subscribe((result) => {
				if (result.isSuccess) {
					Log.debug(`FolderPathService => Fetch Folder Paths`, result.value);

					this.setState({ folderPaths: result.value }, 'Set Folder Paths');
				}
			});
	}

	public getFolderPaths(): Observable<FolderPathDTO[]> {
		return this.stateChanged.pipe(map((state: IStoreState) => state?.folderPaths ?? []));
	}

	public createFolderPath(folderPath: FolderPathDTO): void {
		createFolderPath(folderPath).subscribe((folderPath) => {
			if (folderPath?.isSuccess && folderPath.value) {
				const folderPaths = [...this.getState().folderPaths, ...[folderPath.value]];
				this.setState({ folderPaths }, 'Set Folder Paths');
			}
		});
	}

	public updateFolderPath(folderPath: FolderPathDTO): void {
		updateFolderPath(folderPath)
			.pipe(finalize(() => this.fetchFolderPaths()))
			.subscribe();
	}

	public deleteFolderPath(folderPathId: number): void {
		deleteFolderPath(folderPathId)
			.pipe(finalize(() => this.fetchFolderPaths()))
			.subscribe();
	}
}

const folderPathService = new FolderPathService();
export default folderPathService;
