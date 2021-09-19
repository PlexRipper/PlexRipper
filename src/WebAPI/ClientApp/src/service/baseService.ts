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

	protected updateStore(propertyName: keyof IStoreState, newObject: any, idName: string = 'id'): void {
		const x = this.getState()[propertyName.toString()];
		if (!x) {
			Log.error(`Failed to get IStoreProperty property name: ${propertyName}`, this.getState());
			return;
		}

		if (!newObject[idName]) {
			Log.error(`Failed to find the correct id property in ${propertyName} with idName: ${idName}`, newObject);
			return;
		}

		const i = x.findIndex((x) => x[idName] === newObject[idName]);
		if (i > -1) {
			// Update entry
			x.splice(i, 1, newObject);
		} else {
			// Add new entry
			x.push(newObject);
		}
		const stateObject = {};
		stateObject[propertyName] = x;
		this.setState(stateObject, `Update ${propertyName} with ${idName}: ${newObject[idName]}`);
	}

	protected getStateSliceProperty<TProp>(propertyName: string, deepCloneReturnedState: boolean = true): TProp {
		return super.getStateSliceProperty<TProp>(propertyName, deepCloneReturnedState);
	}
}
