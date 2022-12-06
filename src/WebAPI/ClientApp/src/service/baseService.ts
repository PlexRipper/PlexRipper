import Log from 'consola';
import { Context } from '@nuxt/types';
import { ObservableStore } from '@codewithdan/observable-store';
import { ObservableStoreSettings } from '@codewithdan/observable-store/interfaces';
import { EMPTY, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import IStoreState from '@interfaces/service/IStoreState';
import ISetupResult from '@interfaces/service/ISetupResult';
import IAppConfig from '@class/IAppConfig';
import DefaultState from '@const/default-state';

export default abstract class BaseService extends ObservableStore<IStoreState> {
	protected _nuxtContext!: Context;
	protected _appConfig!: IAppConfig;
	protected _name: string;

	public get name() {
		return this._name;
	}

	protected constructor(serviceName: string, settings: ObservableStoreSettings) {
		settings.trackStateHistory = true;
		super(settings);
		this._name = serviceName;
	}

	public logHistory(): void {
		Log.info(`Current ${this._name} state history:`, this.stateHistory);
	}

	public logState(): void {
		Log.info(`Current state of ${this._name}`, this.getState(true));
	}

	protected setup(nuxtContext, appConfig: IAppConfig | null = null): Observable<ISetupResult> {
		if (!ObservableStore.isStoreInitialized) {
			ObservableStore.initializeState(DefaultState);
			Log.debug('Observable store was initialized');
		}
		this._nuxtContext = nuxtContext;
		if (appConfig !== null) {
			this._appConfig = appConfig;
		}
		Log.debug(`Starting setup of service: ${this._name}`);
		return EMPTY;
	}

	// region WriteStore

	/**
	 * Updates the store property to with the newObject based on its id
	 * Note: Only use this if the store property is an array
	 * @param propertyName
	 * @param newObject
	 * @param idName
	 * @protected
	 */
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

	protected setStoreProperty(propertyName: keyof IStoreState, newValue: any, action: string = ''): void {
		const state = {};
		state[propertyName] = newValue;

		if (!action) {
			action = `The property "${propertyName}" was updated in the store`;
		}
		Log.trace(action, newValue);
		this.setState(state, action);
	}

	// endregion

	// region ReadStore

	/**
	 * A reactive store slice property getter which is a wrapper for `this.getStateSliceProperty` with IStoreState keyOf  propertyNames
	 * @param {string} propertyName
	 * @protected
	 */
	protected getStoreSlice<T>(propertyName: keyof IStoreState): T {
		return this.getStateSliceProperty<T>(propertyName, true);
	}

	// TODO Figure out how to make conditional return type based on the types in IStoreState
	protected getStateChanged<T>(propertyName: keyof IStoreState): Observable<T> {
		return this.stateChanged.pipe(map((x) => x[propertyName] as unknown as T));
	}

	// endregion
}
