import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, computed, toRefs } from 'vue';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { get } from '@vueuse/core';
import type { ISetupResult, I18nObjectType, ILocaleConfig } from '@interfaces';
import { useSettingsStore } from '@store';
import { cloneDeep } from 'lodash-es';
import type { LocaleObject } from '@nuxtjs/i18n';
import type { Locale } from 'vue-i18n';

interface ILocalizationStoreState {
	i18nRef: I18nObjectType;
}

export const useLocalizationStore = defineStore('LocalizationStore', () => {
	// State
	const defaultState: ILocalizationStoreState = {
		i18nRef: {} as I18nObjectType,
	};

	const state = reactive<ILocalizationStoreState>(cloneDeep(defaultState));

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: 'useLocalizationStore', isSuccess: true });
		},
		setI18nObject(i18n?: I18nObjectType) {
			if (!i18n) {
				Log.error('i18n object is not defined');
				return;
			}

			// @ts-expect-error - This is a valid assignment, TypeScript is being retarted here.
			state.i18nRef = i18n;
			actions.changeLanguageLocale(get(i18n.locale));
		},
		changeLanguageLocale(isoCode: Locale) {
			if (!state.i18nRef) {
				Log.error('i18n object is not defined');
				return;
			}
			Log.info('Localization Options:', isoCode);
			state.i18nRef.setLocale(isoCode).then(() => {
				useSettingsStore().languageSettings.language = isoCode;
				Log.info('Localization has been set to:', isoCode);
			});
		},
		toILocalConfig(locale: LocaleObject): ILocaleConfig {
			return {
				text: locale.name!,
				code: locale.code,
				iso: locale.code,
				bcp47Code: locale.code.slice(0, 2),
				img: `/img/flags/${locale.code}.svg`,
			};
		},
		$reset: () => {
		},
	};

	// Getters
	const getters = {
		getLanguageLocale: computed((): ILocaleConfig => {
			const locale = state.i18nRef.locales.find((locale) => locale.code === state.i18nRef.locale) as LocaleObject;
			return actions.toILocalConfig(locale);
		}),
		getLanguageLocaleOptions: computed((): ILocaleConfig[] => {
			return state.i18nRef.locales.map((x) => (actions.toILocalConfig(x)));
		}),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useLocalizationStore, import.meta.hot));
}
