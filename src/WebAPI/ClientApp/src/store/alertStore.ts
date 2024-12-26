import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { of, Subject } from 'rxjs';
import type { ISetupResult, IAlert } from '@interfaces';
import { cloneDeep } from 'lodash-es';

interface IAlertStoreState {
	alerts: IAlert[];
	alertDialogObservable: Subject<IAlert[]>;
}

export const useAlertStore = defineStore('AlertStore', () => {
	const defaultState: IAlertStoreState = {
		alerts: [],
		alertDialogObservable: new Subject<IAlert[]>(),
	};
	const state = reactive<IAlertStoreState>(cloneDeep(defaultState));

	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: 'useAlertStore', isSuccess: true });
		},
		showAlert(alert: IAlert): void {
			const newAlert = { ...alert, id: Date.now() };
			state.alerts.push(newAlert);
			state.alertDialogObservable.next(state.alerts);
		},
		removeAlert(id: number): void {
			state.alerts = state.alerts.filter((x) => x.id !== id);
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};
	const getters = {
		getAlerts: computed((): Observable<IAlert[]> => state.alertDialogObservable),
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useAlertStore, import.meta.hot));
}
