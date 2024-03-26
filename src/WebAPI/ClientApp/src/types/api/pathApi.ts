import { from, Observable, of, throwError } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import type { FolderPathDTO, FileSystemDTO } from '@dto/mainApi';
import type ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { FOLDER_PATH_RELATIVE_PATH } from '@api-urls';
import ApiClient from '@dto/ApiClient';

const logText = 'From folderPathApi => ';

export function getFolderPaths(): Observable<ResultDTO<FolderPathDTO[]>> {
	return PlexRipperAxios.get<FolderPathDTO[]>({
		url: `${FOLDER_PATH_RELATIVE_PATH}`,
		apiCategory: logText,
		apiName: getFolderPaths.name,
	});
}

export function getDirectoryPath(path = ''): Observable<ResultDTO<FileSystemDTO>> {
	return PlexRipperAxios.get<FileSystemDTO>({
		url: `${FOLDER_PATH_RELATIVE_PATH}/directory/?path=${path}`,
		apiCategory: logText,
		apiName: getDirectoryPath.name,
	});
}

export function updateFolderPath(folderPath: FolderPathDTO): Observable<FolderPathDTO> {
	return from(ApiClient.folderPath.updateFolderPathEndpoint(folderPath)).pipe(
		switchMap(({ data }) => {
			if (data.isFailed) {
				return throwError(() => data.errors);
			}
			if (data.value) {
				return of(data.value);
			}

			return throwError(() => 'No value returned');
		}),
	);
}

export function createFolderPath(folderPath: FolderPathDTO): Observable<ResultDTO<FolderPathDTO>> {
	return PlexRipperAxios.post<FolderPathDTO>({
		url: `${FOLDER_PATH_RELATIVE_PATH}`,
		data: folderPath,
		apiCategory: logText,
		apiName: createFolderPath.name,
	});
}

export function deleteFolderPath(folderPathId: number): Observable<ResultDTO<FolderPathDTO>> {
	return PlexRipperAxios.delete<FolderPathDTO>({
		url: `${FOLDER_PATH_RELATIVE_PATH}/${folderPathId}`,
		apiCategory: logText,
		apiName: deleteFolderPath.name,
	});
}
