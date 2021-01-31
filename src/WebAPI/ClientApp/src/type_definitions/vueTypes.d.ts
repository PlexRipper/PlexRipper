import VueI18n from 'vue-i18n';

import Vue from 'vue';
declare module 'vue/types/vue' {
	interface Vue {
		$getThemeClass(): string;

		$messages(): VueI18n.LocaleMessageObject;
		$getMessage(path: string): object;
		$ts(path: string): string;
		$moment(path: string): string;
	}
}

export interface RuntimeConfig {
	nodeEnv: string;
	version: string;
}

declare module '@nuxt/types/config/runtime' {
	interface NuxtRuntimeConfig extends RuntimeConfig {}
}

// Extensions
declare global {
	interface Array<T> {
		addOrReplace(searchFunction: Function, object: any): Array<T>;
		sum(): number;
	}
}

declare module '*.vue' {
	export default Vue;
}
