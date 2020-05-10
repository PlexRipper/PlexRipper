import { NuxtApp } from '@nuxt/types/app';
import { NuxtAxiosInstance } from '@nuxtjs/axios/types';
import { Module, VuexModule } from 'vuex-module-decorators';

// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
@Module({ name: 'globalStore', namespaced: true, stateFactory: true })
export default class GlobalStore extends VuexModule {
	get Nuxt(): NuxtApp {
		return window.$nuxt;
	}

	get Axios(): NuxtAxiosInstance {
		return window.$nuxt.$options.$axios;
	}
}
