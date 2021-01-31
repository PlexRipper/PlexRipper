import Log from 'consola';
import { ObservableStore } from '@codewithdan/observable-store';
import StoreState from '@state/storeState';
import { SettingsModel } from '@dto/mainApi';
import { ObservableStoreSettings } from '@codewithdan/observable-store/interfaces';

export class BaseService extends ObservableStore<StoreState> {
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
			} as StoreState);
		}
	}

	public logHistory(): void {
		Log.warn('history', this.stateHistory);
	}
}
