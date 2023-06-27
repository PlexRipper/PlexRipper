import { acceptHMRUpdate } from 'pinia';

export const useServerStore = defineStore('ServerStore', {
	state: (): {} => ({}),
	actions: {},
	getters: {},
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useServerStore, import.meta.hot));
}
