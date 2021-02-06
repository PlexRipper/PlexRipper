import Log from 'consola';
import { ObservableStore } from '@codewithdan/observable-store';
import IStoreState from '@interfaces/IStoreState';
import { SettingsModel } from '@dto/mainApi';
import { ObservableStoreSettings } from '@codewithdan/observable-store/interfaces';

export class BaseService extends ObservableStore<IStoreState> {
	public constructor(settings: ObservableStoreSettings) {
		super(settings);

		if (!this.getState()) {
			ObservableStore.initializeState({
				accounts: [],
				servers: [],
				downloads: [],
				libraries: [],
				mediaUrls: [],
				settings: {} as SettingsModel,
			} as IStoreState);
		}
	}

	public logHistory(): void {
		Log.warn('history', this.stateHistory);
	}
}
