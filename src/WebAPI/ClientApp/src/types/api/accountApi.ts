import { Observable } from 'rxjs';
import ResultDTO from '@dto/ResultDTO';
import { AuthPin, PlexAccountDTO } from '@dto/mainApi';
import PlexRipperAxios from '@class/PlexRipperAxios';

const logText = 'From AccountAPI => ';
const apiPath = '/plexaccount';

export function getAllAccounts(): Observable<ResultDTO<PlexAccountDTO[]>> {
	return PlexRipperAxios.get<PlexAccountDTO[]>({
		url: `${apiPath}`,
		apiCategory: logText,
		apiName: getAllAccounts.name,
	});
}

export function getAllEnabledAccounts(): Observable<ResultDTO<PlexAccountDTO[]>> {
	return PlexRipperAxios.get<PlexAccountDTO[]>({
		url: `${apiPath}/?enabledOnly=true`,
		apiCategory: logText,
		apiName: getAllEnabledAccounts.name,
	});
}

export function validateAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO>> {
	return PlexRipperAxios.post<PlexAccountDTO>({
		url: `${apiPath}/validate`,
		apiCategory: logText,
		apiName: validateAccount.name,
		data: account,
	});
}

export function createAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO | null>> {
	return PlexRipperAxios.post<PlexAccountDTO>({
		url: `${apiPath}`,
		apiCategory: logText,
		apiName: createAccount.name,
		data: account,
	});
}

export function updateAccount(account: PlexAccountDTO, inspect: boolean = false): Observable<ResultDTO<PlexAccountDTO | null>> {
	return PlexRipperAxios.put<PlexAccountDTO | null>({
		url: `${apiPath}/${account.id}?inspect=${inspect}`,
		apiCategory: logText,
		apiName: updateAccount.name,
		data: account,
	});
}

export function deleteAccount(accountId: Number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.delete<boolean>({
		url: `${apiPath}/${accountId}`,
		apiCategory: logText,
		apiName: deleteAccount.name,
	});
}

export function getAccount(accountId: Number): Observable<ResultDTO<PlexAccountDTO>> {
	return PlexRipperAxios.get<PlexAccountDTO>({
		url: `${apiPath}/${accountId}`,
		apiCategory: logText,
		apiName: getAccount.name,
	});
}

export function refreshAccount(accountId: Number): Observable<ResultDTO> {
	return PlexRipperAxios.get<void>({
		url: `${apiPath}/refresh/${accountId}`,
		apiCategory: logText,
		apiName: refreshAccount.name,
	});
}

export function GetAndCheck2FaPin(clientId: String, authPinId: number = 0): Observable<ResultDTO<AuthPin>> {
	return PlexRipperAxios.get<AuthPin>({
		url: `${apiPath}/authpin`,
		apiCategory: logText,
		apiName: GetAndCheck2FaPin.name,
		params: {
			clientId,
			authPinId,
		},
	});
}

export function checkAuthPin(clientId: String): Observable<ResultDTO<AuthPin>> {
	return PlexRipperAxios.get<AuthPin>({
		url: `${apiPath}/authpin/${clientId}/check`,
		apiCategory: logText,
		apiName: checkAuthPin.name,
	});
}

export function generateClientId(): Observable<ResultDTO<string>> {
	return PlexRipperAxios.get<string>({
		url: `${apiPath}/clientid`,
		apiCategory: logText,
		apiName: generateClientId.name,
	});
}
