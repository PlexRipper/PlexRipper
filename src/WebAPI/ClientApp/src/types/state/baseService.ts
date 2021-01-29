import Log from 'consola';
import { ObservableStore } from '@codewithdan/observable-store';
import StoreState from '@state/storeState';
import { DownloadTaskContainerDTO } from '@dto/mainApi';

export class BaseService extends ObservableStore<StoreState> {
	public constructor(stateSliceSelector: (state: any) => any = () => {}) {
		super({ trackStateHistory: true, stateSliceSelector });

		if (!this.getState()) {
			ObservableStore.initializeState({
				servers: [],
				downloads: {} as DownloadTaskContainerDTO,
				libraries: [],
				mediaUrls: [],
			} as StoreState);
		}
	}

	public logHistory(): void {
		Log.warn('history', this.stateHistory);
	}
}
