import { ObservableStore } from '@codewithdan/observable-store';
import StoreState from '@state/storeState';
import Log from 'consola';

export class BaseService extends ObservableStore<StoreState> {
	public constructor(stateSliceSelector: (state: any) => any = () => {}) {
		super({ trackStateHistory: true, stateSliceSelector });

		if (!this.getState()) {
			ObservableStore.initializeState({
				servers: [],
				downloads: [],
				libraries: [],
			} as StoreState);
		}
	}

	public logHistory(): void {
		Log.warn('history', this.stateHistory);
	}
}
