import Vue from 'vue';
import Vuex from 'vuex';
import { getModule, config } from 'vuex-module-decorators';

// Modules
import SettingsStore from '~/store/settingsStore';

config.rawError = true;

// The Vuex store must be defined in Vue before anything else!
Vue.use(Vuex);

interface StoreType {
	settingsStore: SettingsStore;
}

// This will allows access in each component without having to initialize in each component.
// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
export const store = new Vuex.Store<StoreType>({
	modules: {
		SettingsStore,
	},
});

export const settingsStore: SettingsStore = getModule(SettingsStore, store);
