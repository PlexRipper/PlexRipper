import { Observable } from 'rxjs';
import type { SettingsModelDTO } from '@dto/mainApi';
import type ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { SETTINGS_RELATIVE_PATH } from '@api-urls';

const logText = 'From SettingsAPI => ';

export function getSettings(): Observable<ResultDTO<SettingsModelDTO>> {
	return PlexRipperAxios.get<SettingsModelDTO>({
		url: SETTINGS_RELATIVE_PATH,
		apiCategory: logText,
		apiName: getSettings.name,
	});
}

export function updateSettings(settings: SettingsModelDTO): Observable<ResultDTO<SettingsModelDTO>> {
	return PlexRipperAxios.put<SettingsModelDTO>({
		url: SETTINGS_RELATIVE_PATH,
		apiCategory: logText,
		apiName: updateSettings.name,
		data: settings,
	});
}

export function resetDatabase(): Observable<ResultDTO> {
	return PlexRipperAxios.get<void>({
		url: `${SETTINGS_RELATIVE_PATH}/ResetDb`,
		apiCategory: logText,
		apiName: resetDatabase.name,
	});
}
