import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { of, Subject } from 'rxjs';
import type { ISetupResult } from '@interfaces';
import type IAlert from '@interfaces/IAlert';

export const useAlertStore = defineStore('AlertStore', () => {
	const state = reactive<{ alerts: IAlert[]; alertDialogObservable: Subject<IAlert[]> }>({
		alerts: [],
		alertDialogObservable: new Subject<IAlert[]>(),
	});
	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: useAlertStore.name, isSuccess: true });
		},
		showAlert(alert: IAlert): void {
			const newAlert = { ...alert, id: Date.now() };
			state.alerts.push(newAlert);
			state.alertDialogObservable.next(state.alerts);
		},
		removeAlert(id: number): void {
			state.alerts = state.alerts.filter((x) => x.id !== id);
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
