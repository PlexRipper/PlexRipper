import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, computed, toRefs } from 'vue';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { get } from '@vueuse/core';
import { StoreNames, type ISetupResult, type I18nObjectType, type ILocaleConfig } from '@interfaces';
import { useSettingsStore } from '@store';
import { cloneDeep } from 'lodash-es';
import type { LocaleObject } from '@nuxtjs/i18n';
import type { Locale } from 'vue-i18n';

interface ILocalizationStoreState {
	i18nRef: I18nObjectType;
}

const emptyLocaleConfig: ILocaleConfig = {
	text: '',
	code: '' as Locale,
	iso: '',
	bcp47Code: '',
	img: '',
};

export const useLocalizationStore = defineStore(StoreNames.LocalizationStore, () => {
	// State
	const defaultState: ILocalizationStoreState = {
		i18nRef: {} as I18nObjectType,
	};

	const state = reactive<ILocalizationStoreState>(cloneDeep(defaultState));

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: StoreNames.LocalizationStore, isSuccess: true });
		},
		setI18nObject(i18n?: I18nObjectType) {
			if (!i18n) {
				Log.error('i18n object is not defined');
				return;
			}

			// @ts-expect-error - i18n type from plugin is narrower than runtime object.
			state.i18nRef = i18n;
			actions.changeLanguageLocale(get(i18n.locale));
		},
		changeLanguageLocale(isoCode: Locale) {
			if (!state.i18nRef || typeof state.i18nRef.setLocale !== 'function') {
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
			const locales = state.i18nRef?.locales;
			if (!Array.isArray(locales) || locales.length === 0) {
				return emptyLocaleConfig;
			}

			const currentLocale = get(state.i18nRef.locale) as Locale;
			const locale = locales.find((x) => x.code === currentLocale);

			if (!locale) {
				return emptyLocaleConfig;
			}

			return actions.toILocalConfig(locale as LocaleObject);
		}),
		getLanguageLocaleOptions: computed((): ILocaleConfig[] => {
			const locales = state.i18nRef?.locales;
			if (!Array.isArray(locales) || locales.length === 0) {
				return [];
			}

			return locales.map((x) => (actions.toILocalConfig(x as LocaleObject)));
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
