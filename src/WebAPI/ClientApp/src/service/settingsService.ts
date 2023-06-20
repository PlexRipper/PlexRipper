import { Observable, of } from 'rxjs';
import { map, switchMap, take, tap } from 'rxjs/operators';
import BaseService from './baseService';
import { SettingsModelDTO } from '@dto/mainApi';
import { getSettings, updateSettings } from '@api/settingsApi';
import ISetupResult from '@interfaces/service/ISetupResult';
import { useSettingsStore } from '~/store';

export class SettingsService extends BaseService {
	// region Constructor and Setup
	public constructor() {
		super('SettingsService', {});
	}

	public setup(): Observable<ISetupResult> {
		super.setup();

		// On app load, request the settings once
		return this.fetchSettings().pipe(
			switchMap(() => of({ name: this._name, isSuccess: true })),
			tap(() => {
				// Send the settings to the server when they change
				useSettingsStore().$subscribe((_, state) => {
					updateSettings(state).subscribe();
				});
			}),
			take(1),
		);
	}
	// endregion

	// region Fetch
	public fetchSettings(): Observable<SettingsModelDTO | null> {
		return getSettings().pipe(
			switchMap((settingsResult) => of(settingsResult?.value ?? null)),
			tap((settings) => {
				if (settings) {
					useSettingsStore().setSettingsState(settings);
				}
			}),
		);
	}

	// endregion
}

export default new SettingsService();
