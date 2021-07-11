import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { FolderPathDTO, FileSystemDTO } from '@dto/mainApi';
import { checkResponse, preApiRequest } from '@api/baseApi';
import ResultDTO from '@dto/ResultDTO';

const logText = 'From folderPathApi => ';
const apiPath = '/folderpath';

export function getFolderPaths(): Observable<ResultDTO<FolderPathDTO[]>> {
	preApiRequest(logText, 'getFolderPaths');
	const result = Axios.get(`${apiPath}`);
	return checkResponse<ResultDTO<FolderPathDTO[]>>(result, logText, 'getFolderPaths');
}

export function getDirectoryPath(path: string = ''): Observable<ResultDTO<FileSystemDTO>> {
	preApiRequest(logText, 'getDirectoryPath');
	const result = Axios.get(`${apiPath}/directory/?path=${path}`);
	return checkResponse<ResultDTO<FileSystemDTO>>(result, logText, 'getDirectoryPath');
}

export function updateFolderPath(folderPath: FolderPathDTO): Observable<ResultDTO<FolderPathDTO>> {
	preApiRequest(logText, 'updateFolderPath');
	const result = Axios.put(`${apiPath}`, folderPath);
	return checkResponse<ResultDTO<FolderPathDTO>>(result, logText, 'updateFolderPath');
}
