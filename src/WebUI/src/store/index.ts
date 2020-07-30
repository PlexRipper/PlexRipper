import Vue from 'vue';
import Vuex from 'vuex';
import { getModule, config } from 'vuex-module-decorators';

// Modules
import globalStore from '@/store/globalStore';
import userStore from '@/store/userStore';

config.rawError = true;

// The Vuex store must be defined in Vue before anything else!
Vue.use(Vuex);

// This will allows access in each component without having to initialize in each component.
// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
export const store = new Vuex.Store<unknown>({
	modules: {
		globalStore,
		userStore,
	},
});

export const GlobalStore: globalStore = getModule(globalStore, store);
export const UserStore: userStore = getModule(userStore, store);
