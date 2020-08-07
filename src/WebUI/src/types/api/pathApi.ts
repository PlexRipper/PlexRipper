import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { FolderPathDTO, FileSystemDTO } from '@dto/mainApi';
import { checkResponse, preApiRequest } from '@api/baseApi';

const logText = 'From folderPathApi => ';
const apiPath = '/folderpath';

export function getFolderPaths(): Observable<FolderPathDTO[]> {
	preApiRequest(logText, 'getFolderPaths');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}`);
	return checkResponse<FolderPathDTO[]>(result, logText, 'getFolderPaths');
}

export function getDirectoryPath(path: string): Observable<FileSystemDTO> {
	preApiRequest(logText, 'getDirectoryPath');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/directory/?path=${path}`);
	return checkResponse<FileSystemDTO>(result, logText, 'getDirectoryPath');
}

export function updateFolderPath(folderPath: FolderPathDTO): Observable<FolderPathDTO> {
	preApiRequest(logText, 'updateFolderPath');
	const result: Observable<AxiosResponse> = Axios.put(`${apiPath}`, folderPath);
	return checkResponse<FolderPathDTO>(result, logText, 'updateFolderPath');
}
