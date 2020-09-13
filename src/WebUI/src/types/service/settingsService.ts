import { ReplaySubject, Observable } from 'rxjs';
import { tap, switchMap, finalize } from 'rxjs/operators';
import { getSettings, updateSettings } from '@api/settingsApi';
import { SettingsModel } from '@dto/mainApi';
import GlobalService from '@service/globalService';
import Log from 'consola';

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
	}

	public getSettings(): Observable<SettingsModel> {
		return this._settings.asObservable();
	}

	public updateSettings(settings: SettingsModel): void {
		updateSettings(settings)
			.pipe(finalize(() => this.fetchSettings()))
			.subscribe();
	}

	public fetchSettings(): void {
		getSettings().subscribe((value) => {
			this._settings.next(value);
		});
	}
}

const settingsService = new SettingsService();
export default settingsService;
