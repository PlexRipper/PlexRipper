import { ReplaySubject, Observable } from 'rxjs';
import { tap, switchMap, finalize } from 'rxjs/operators';
import { getSettings, updateSettings } from '@api/settingsApi';
import { SettingsModelDTO } from '@dto/mainApi';
import GlobalService from '@service/globalService';
import Log from 'consola';

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
	}

	public getSettings(): Observable<SettingsModelDTO> {
		return this._settings.asObservable();
	}

	public updateSettings(settings: SettingsModelDTO): void {
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
