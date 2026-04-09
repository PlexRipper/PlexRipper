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

export const useLocalizationStore = defineStore(StoreNames.LocalizationStore, () => {
	// State
	const defaultState: ILocalizationStoreState = {
		i18nRef: {} as I18nObjectType,
	};

	const state = reactive<ILocalizationStoreState>(cloneDeep(defaultState));
	const emptyLocale: ILocaleConfig = {
		text: '',
		code: '' as Locale,
		iso: '',
		bcp47Code: '',
		img: '',
	};

	function isI18nReady(i18n?: unknown): i18n is I18nObjectType {
		return !!i18n
			&& Array.isArray((i18n as { locales?: unknown }).locales)
			&& typeof (i18n as { setLocale?: unknown }).setLocale === 'function';
	}

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: StoreNames.LocalizationStore, isSuccess: true });
		},
		setI18nObject(i18n?: unknown) {
			if (!isI18nReady(i18n)) {
				Log.error('i18n object is not defined');
				return;
			}

			// @ts-expect-error - This is a valid assignment, TypeScript is being retarted here.
			state.i18nRef = i18n;
			actions.changeLanguageLocale(get(i18n.locale));
		},
		changeLanguageLocale(isoCode: Locale) {
			if (!isI18nReady(state.i18nRef)) {
				Log.error('i18n object is not defined');
				return;
			}
			Log.info('Localization Options:', isoCode);
			state.i18nRef.setLocale(isoCode).then(() => {
				useSettingsStore().languageSettings.language = isoCode;
				Log.info('Localization has been set to:', isoCode);
			});
		},
		toILocalConfig(locale?: LocaleObject | null): ILocaleConfig {
			if (!locale?.code || !locale?.name) {
				return emptyLocale;
			}
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
			if (!isI18nReady(state.i18nRef)) {
				return emptyLocale;
			}

			const locale = state.i18nRef.locales.find((locale) => locale.code === state.i18nRef.locale) as LocaleObject | undefined;
			return actions.toILocalConfig(locale);
		}),
		getLanguageLocaleOptions: computed((): ILocaleConfig[] => {
			if (!isI18nReady(state.i18nRef)) {
				return [];
			}

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
