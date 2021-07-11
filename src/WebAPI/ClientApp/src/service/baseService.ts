import Log from 'consola';
import { Context } from '@nuxt/types';
import { ObservableStore } from '@codewithdan/observable-store';
import IStoreState from '@interfaces/IStoreState';
import { ObservableStoreSettings } from '@codewithdan/observable-store/interfaces';

export default class BaseService extends ObservableStore<IStoreState> {
	protected _nuxtContext!: Context;

	public constructor(settings: ObservableStoreSettings) {
		settings.trackStateHistory = true;
		super(settings);
	}

	public setup(nuxtContext: Context): void {
		this._nuxtContext = nuxtContext;
	}

	public logHistory(): void {
		Log.warn('history', this.stateHistory);
	}
}
