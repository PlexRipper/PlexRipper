import { acceptHMRUpdate, defineStore } from 'pinia';
import { Observable, of, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import type { ISetupResult } from '@interfaces';

export const useHelpStore = defineStore('HelpStore', () => {
	const state = reactive<{ helpIdDialog: string; helpDialogObservable: Subject<string> }>({
		helpIdDialog: '',
		helpDialogObservable: new Subject<string>(),
	});
	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: useHelpStore.name, isSuccess: true });
		},
		openHelpDialog(helpId: string): void {
			state.helpIdDialog = helpId;
			state.helpDialogObservable.next(helpId);
		},
	};
	const getters = {
		getHelpDialog: computed((): Observable<string> => state.helpDialogObservable.pipe(map((x) => x ?? ''))),
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useHelpStore, import.meta.hot));
}
