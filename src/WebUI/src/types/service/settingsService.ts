import Log from 'consola';
import { ReplaySubject, Observable } from 'rxjs';
import { tap, switchMap, finalize } from 'rxjs/operators';
import { getSettings, updateSettings } from '@api/settingsApi';
import { SettingsModel } from '@dto/mainApi';
import GlobalService from '@state/globalService';
import { settingsStore as SettingsStore } from '@/store';

export class SettingsService {
	private _settings: ReplaySubject<SettingsModel> = new ReplaySubject<SettingsModel>();

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
		this.getSettings().subscribe((data) => SettingsStore.setSettings(data));
	}

	public getSettings(): Observable<SettingsModel> {
		return this._settings.asObservable();
	}

	public updateSettings(settings: SettingsModel): void {
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
