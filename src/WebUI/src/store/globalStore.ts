import { NuxtApp } from '@nuxt/types/app';
import { Module, VuexModule } from 'vuex-module-decorators';
import VueI18n, { IVueI18n } from 'vue-i18n';
import { NuxtVueI18n } from 'nuxt-i18n/types/nuxt-i18n';

@Module({
	name: 'globalStore',
	namespaced: true,
	stateFactory: true,
})
export default class GlobalStore extends VuexModule {
	get Nuxt(): NuxtApp {
		return window.$nuxt;
	}

	get i18n(): VueI18n & IVueI18n {
		return window.$nuxt.$i18n;
	}

	get getAvailableLanguageCodes(): string[] {
		const langs: string[] = [];

		if (window.$nuxt.$i18n.locales) {
			window.$nuxt.$i18n.locales.forEach((locale: string | NuxtVueI18n.Options.LocaleObject) => {
				langs.push((locale as NuxtVueI18n.Options.LocaleObject).code);
			});
		}

		return langs;
	}

	debugMode = true;

	get isDebugMode(): boolean {
		return this.debugMode;
	}
}
