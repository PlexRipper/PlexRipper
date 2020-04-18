/* eslint-disable @typescript-eslint/interface-name-prefix */
/* eslint-disable @typescript-eslint/no-explicit-any */
import { Store } from 'vuex';
import { Consola } from 'consola';

declare module 'vue/types/vue' {
	interface VueConstructor {
		$log: Consola;
		$store: Store<any>;
	}
}

declare module '@nuxt/types' {
	interface NuxtAppOptions {
		$log: Consola;
	}
	interface Context {
		$log: Consola;
	}
}

declare module 'vuex/types/index' {
	interface Store<S> {
		$log: Consola;
	}
}
