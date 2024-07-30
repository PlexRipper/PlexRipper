import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { of, Subject } from 'rxjs';
import type { IHelp, ISetupResult } from '@interfaces';

export const useHelpStore = defineStore('HelpStore', () => {
	const state = reactive<{ helpIdDialog: IHelp; helpDialogObservable: Subject<IHelp> }>({
		helpIdDialog: { label: '', title: '', text: '' },
		helpDialogObservable: new Subject<IHelp>(),
	});
	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: useHelpStore.name, isSuccess: true });
		},
		openHelpDialog(help: IHelp): void {
			if (!help) {
				return;
			}
			state.helpIdDialog = help;
			state.helpDialogObservable.next(help);
		},
	};
	const getters = {
		getHelpDialog: computed((): Observable<IHelp> => state.helpDialogObservable),
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
