import Log from 'consola';
import { IFileSystem } from '@dto/settings/paths/IFileSystem';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import IFolderPath from '~/types/dto/settings/iFolderPath';

const logText = 'From folderPathApi => ';
const apiPath = '/folderpath';

export function getFolderPaths(): Observable<IFolderPath[]> {
	Log.debug(`${logText}getFolderPaths: Sending request`);
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}`);

	return result.pipe(
		tap((res) => Log.debug(`${logText}getFolderPaths response:`, res.data)),
		map((res: AxiosResponse) => res.data),
	);
}

export function getDirectoryPath(path: string): Observable<IFileSystem> {
	Log.debug(`${logText}getDirectoryPath: Sending request: ${path}`);
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/directory/?path=${path}`);

	return result.pipe(
		tap((res) => Log.debug(`${logText}getDirectoryPath response:`, res.data)),
		map((res: AxiosResponse) => res.data),
	);
}

export function updateFolderPath(folderPath: IFolderPath): Observable<IFolderPath> {
	Log.debug(`${logText}updateFolderPath: Sending request: ${folderPath}`);
	const result: Observable<AxiosResponse> = Axios.put(`${apiPath}`, folderPath);

	return result.pipe(
		tap((res) => Log.debug(`${logText}updateFolderPath response:`, res.data)),
		map((res: AxiosResponse) => res.data),
	);
}
