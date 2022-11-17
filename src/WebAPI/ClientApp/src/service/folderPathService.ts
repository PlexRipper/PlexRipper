import { Observable } from 'rxjs';
import { finalize, map, switchMap, take, tap } from 'rxjs/operators';
import { Context } from '@nuxt/types';
import { FolderPathDTO } from '@dto/mainApi';
import { BaseService } from '@service';
import { getFolderPaths, createFolderPath, updateFolderPath, deleteFolderPath } from '@api/pathApi';
import IStoreState from '@interfaces/service/IStoreState';

export class FolderPathService extends BaseService {
	public constructor() {
		super('FolderPathService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					folderPaths: state.folderPaths,
				};
			},
		});
	}

	public setup(nuxtContext: Context): Observable<any> {
		super.setNuxtContext(nuxtContext);

		return this.refreshFolderPaths().pipe(take(1));
	}

	public refreshFolderPaths(): Observable<FolderPathDTO[]> {
		return getFolderPaths().pipe(
			tap((folderPaths) => {
				if (folderPaths.isSuccess) {
					this.setStoreProperty('folderPaths', folderPaths.value);
				}
			}),
			switchMap(() => this.getFolderPaths()),
		);
	}

	public fetchFolderPaths(): void {
		this.refreshFolderPaths().subscribe();
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
