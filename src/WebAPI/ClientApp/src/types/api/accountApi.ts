import { Observable } from 'rxjs';
import type ResultDTO from '@dto/ResultDTO';
import type { AuthPin, PlexAccountDTO } from '@dto/mainApi';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { PLEX_ACCOUNT_RELATIVE_PATH } from '@api-urls';

const logText = 'From AccountAPI => ';

export function getAllAccounts(): Observable<ResultDTO<PlexAccountDTO[]>> {
	return PlexRipperAxios.get<PlexAccountDTO[]>({
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}`,
		apiCategory: logText,
		apiName: getAllAccounts.name,
	});
}

export function getAllEnabledAccounts(): Observable<ResultDTO<PlexAccountDTO[]>> {
	return PlexRipperAxios.get<PlexAccountDTO[]>({
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}/?enabledOnly=true`,
		apiCategory: logText,
		apiName: getAllEnabledAccounts.name,
	});
}

export function validateAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO>> {
	return PlexRipperAxios.post<PlexAccountDTO>({
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}/validate`,
		apiCategory: logText,
		apiName: validateAccount.name,
		data: account,
	});
}

export function createAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO | null>> {
	return PlexRipperAxios.post<PlexAccountDTO>({
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}`,
		apiCategory: logText,
		apiName: createAccount.name,
		data: account,
	});
}

export function updateAccount(account: PlexAccountDTO, inspect = false): Observable<ResultDTO<PlexAccountDTO | null>> {
	return PlexRipperAxios.put<PlexAccountDTO | null>({
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}/${account.id}?inspect=${inspect}`,
		apiCategory: logText,
		apiName: updateAccount.name,
		data: account,
	});
}

export function deleteAccount(accountId: Number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.delete<boolean>({
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}/${accountId}`,
		apiCategory: logText,
		apiName: deleteAccount.name,
	});
}

export function getAccount(accountId: Number): Observable<ResultDTO<PlexAccountDTO>> {
	return PlexRipperAxios.get<PlexAccountDTO>({
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}/${accountId}`,
		apiCategory: logText,
		apiName: getAccount.name,
	});
}

export function refreshAccount(accountId: Number): Observable<ResultDTO> {
	return PlexRipperAxios.get<void>({
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}/refresh/${accountId}`,
		apiCategory: logText,
		apiName: refreshAccount.name,
	});
}

export function GetAndCheck2FaPin(clientId: String, authPinId = 0): Observable<ResultDTO<AuthPin>> {
	return PlexRipperAxios.get<AuthPin>({
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}/authpin`,
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
		url: `${PLEX_ACCOUNT_RELATIVE_PATH}/authpin/${clientId}/check`,
		apiCategory: logText,
		apiName: checkAuthPin.name,
	});
}
