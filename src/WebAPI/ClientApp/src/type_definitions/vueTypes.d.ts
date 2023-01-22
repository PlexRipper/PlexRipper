import VueI18n from 'vue-i18n';

declare module 'vue/types/vue' {
	interface Vue {
		$getThemeClass(): string;

		$messages(): VueI18n.LocaleMessageObject;

		$getMessage(path: string): object;

		$ts(path: string, values?: VueI18n.Values): string;
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

		last(): T;
	}
}
