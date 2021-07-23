import Log from 'consola';
import { Context } from '@nuxt/types';
import { ObservableStore } from '@codewithdan/observable-store';
import IStoreState from '@interfaces/IStoreState';
import { ObservableStoreSettings } from '@codewithdan/observable-store/interfaces';

export default abstract class BaseService extends ObservableStore<IStoreState> {
	protected _nuxtContext!: Context;

	protected constructor(settings: ObservableStoreSettings) {
		settings.trackStateHistory = true;
		super(settings);
	}

	public setup(nuxtContext: Context): void {
		this._nuxtContext = nuxtContext;
	}

	public logHistory(): void {
		Log.warn('history', this.stateHistory);
	}

	protected updateStore(propertyName: string, newObject: any): void {
		const x = this.getState()[propertyName];
		const i = x.findIndex((x) => x.id === newObject.id);
		if (i > -1) {
			// Update entry
			x.splice(i, 1, newObject);
		} else {
			// Add new entry
			x.push(newObject);
		}

		const stateObject = {};
		stateObject[propertyName] = x;
		this.setState(stateObject, `Update ${propertyName}`);
	}
}
