import { ReplaySubject, Observable } from 'rxjs';
import { tap, switchMap, finalize } from 'rxjs/operators';
import { getSettings, updateSettings } from '@api/settingsApi';
import { SettingsModelDTO } from '@dto/mainApi';
import GlobalService from '@service/globalService';
import Log from 'consola';
import SettingsStore from '@/store/settingsStore';

export class SettingsService {
	private _settings: ReplaySubject<SettingsModelDTO> = new ReplaySubject<SettingsModelDTO>();

	public constructor() {
		GlobalService.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving settings')),
				switchMap(() => getSettings()),
			)
			.subscribe((settings) => {
				this._settings.next(settings);
			});

		// Update the Vuex store with settings
		// this.getSettings().subscribe((data) => SettingsStore.setSettings(data));
	}

	public getSettings(): Observable<SettingsModelDTO> {
		return this._settings.asObservable();
	}

	public updateSettings(settings: SettingsModelDTO): void {
		if (settings) {
			updateSettings(settings)
				.pipe(finalize(() => this.fetchSettings()))
				.subscribe();
		} else {
			Log.warn('SettingsService => updateSettings: settings was invalid, will not send as an update.');
		}
	}

	public fetchSettings(): void {
		getSettings().subscribe((value) => {
			this._settings.next(value);
		});
	}
}

const settingsService = new SettingsService();
export default settingsService;
