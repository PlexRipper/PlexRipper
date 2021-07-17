import { Observable } from 'rxjs';
import { FolderPathDTO } from '@dto/mainApi';
import { map, switchMap } from 'rxjs/operators';
import { BaseService, GlobalService } from '@service';
import { getFolderPaths, createFolderPath, deleteFolderPath } from '@api/pathApi';
import IStoreState from '@interfaces/IStoreState';
import { Context } from '@nuxt/types';

export class FolderPathService extends BaseService {
	public constructor() {
		super({
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
			.pipe(switchMap(() => getFolderPaths()))
			.subscribe((result) => {
				if (result.isSuccess) {
					this.setState({ folderPaths: result.value }, 'Set Folder Paths');
				}
			});
	}

	public getFolderPaths(): Observable<FolderPathDTO[]> {
		return this.stateChanged.pipe(map((state: IStoreState) => state?.folderPaths ?? []));
	}

	public deleteFolderPath(folderPathId: number): void {
		deleteFolderPath(folderPathId)
			.pipe(switchMap(() => getFolderPaths()))
			.subscribe((result) => {
				if (result.isSuccess) {
					this.setState({ folderPaths: result.value }, 'Set Folder Paths');
				}
			});
	}

	public createFolderPath(folderPath: FolderPathDTO): void {
		createFolderPath(folderPath).subscribe((folderPath) => {
			if (folderPath?.isSuccess && folderPath.value) {
				const folderPaths = [...this.getState().folderPaths, ...[folderPath.value]];
				this.setState({ folderPaths }, 'Set Folder Paths');
			}
		});
	}
}

const folderPathService = new FolderPathService();
export default folderPathService;
