import Log from 'consola';
import { ObservableStore } from '@codewithdan/observable-store';
import StoreState from '@state/storeState';

export class BaseService extends ObservableStore<StoreState> {
	public constructor(stateSliceSelector: (state: any) => any = () => {}) {
		super({ trackStateHistory: true, stateSliceSelector });

		if (!this.getState()) {
			ObservableStore.initializeState({
				servers: [],
				downloads: [],
				libraries: [],
				mediaUrls: [],
			} as StoreState);
		}
	}

	public logHistory(): void {
		Log.warn('history', this.stateHistory);
	}
}
