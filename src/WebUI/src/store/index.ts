import Vue from 'vue';
import Vuex from 'vuex';
import { getModule } from 'vuex-module-decorators';

// Modules
import globalStore from '@/store/globalStore';

// The Vuex store must be defined in Vue before anything else!
Vue.use(Vuex);

// This will allows access in each component without having to initialize in each component.
// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
export const store = new Vuex.Store<unknown>({
	modules: {
		globalStore,
	},
	// Register the Vuex-ORM database, this will auto-install all used modules.
});

export const GlobalStore: globalStore = getModule(globalStore, store);
