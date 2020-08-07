import { IFileSystem } from '@dto/settings/paths/IFileSystem';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { checkResponse, preApiRequest } from './baseApi';
import IFolderPath from '~/types/dto/settings/iFolderPath';

const logText = 'From folderPathApi => ';
const apiPath = '/folderpath';

export function getFolderPaths(): Observable<IFolderPath[]> {
	preApiRequest(logText, 'getFolderPaths');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}`);
	return checkResponse<IFolderPath[]>(result, logText, 'getFolderPaths');
}

export function getDirectoryPath(path: string): Observable<IFileSystem> {
	preApiRequest(logText, 'getDirectoryPath');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/directory/?path=${path}`);
	return checkResponse<IFileSystem>(result, logText, 'getDirectoryPath');
}

export function updateFolderPath(folderPath: IFolderPath): Observable<IFolderPath> {
	preApiRequest(logText, 'updateFolderPath');
	const result: Observable<AxiosResponse> = Axios.put(`${apiPath}`, folderPath);
	return checkResponse<IFolderPath>(result, logText, 'updateFolderPath');
}
