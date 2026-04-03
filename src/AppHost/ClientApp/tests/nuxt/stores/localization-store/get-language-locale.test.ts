import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { ref } from 'vue';
import type { LocaleObject } from '@nuxtjs/i18n';
import { baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import Log from 'consola';
import { useLocalizationStore } from '@store';
import type { I18nObjectType } from '@interfaces';

describe('LocalizationStore.getLanguageLocale', () => {
	let { mock: _mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		_mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return locale that matches current i18n locale ref', () => {
		// Arrange
		const localizationStore = useLocalizationStore();
		const i18n = {
			locale: ref('en-US'),
			locales: [
				{ code: 'en-US', name: 'English' },
				{ code: 'nl-NL', name: 'Nederlands' },
			] as LocaleObject[],
			setLocale: vi.fn().mockResolvedValue(undefined),
		} as unknown as I18nObjectType;

		// Act
		localizationStore.setI18nObject(i18n);

		// Assert
		expect(localizationStore.getLanguageLocale.code).toEqual('en-US');
		expect(localizationStore.getLanguageLocale.text).toEqual('English');
	});

	test('Should return an empty fallback locale when i18n object is not set', () => {
		// Arrange
		const localizationStore = useLocalizationStore();

		// Act + Assert
		expect(() => localizationStore.getLanguageLocale).not.toThrow();
		expect(localizationStore.getLanguageLocale).toEqual({
			text: '',
			code: '',
			iso: '',
			bcp47Code: '',
			img: '',
		});
		expect(localizationStore.getLanguageLocaleOptions).toEqual([]);
	});

	test('Should return an empty fallback locale when current locale is not in available locales', () => {
		// Arrange
		const localizationStore = useLocalizationStore();
		const i18n = {
			locale: ref('fr-FR'),
			locales: [
				{ code: 'en-US', name: 'English' },
				{ code: 'nl-NL', name: 'Nederlands' },
			] as LocaleObject[],
			setLocale: vi.fn().mockResolvedValue(undefined),
		} as unknown as I18nObjectType;

		// Act
		localizationStore.setI18nObject(i18n);

		// Assert
		expect(localizationStore.getLanguageLocale).toEqual({
			text: '',
			code: '',
			iso: '',
			bcp47Code: '',
			img: '',
		});
	});

	test('Should map locale options including bcp47Code and img', () => {
		// Arrange
		const localizationStore = useLocalizationStore();
		const i18n = {
			locale: ref('nl-NL'),
			locales: [
				{ code: 'en-US', name: 'English' },
				{ code: 'nl-NL', name: 'Nederlands' },
			] as LocaleObject[],
			setLocale: vi.fn().mockResolvedValue(undefined),
		} as unknown as I18nObjectType;

		// Act
		localizationStore.setI18nObject(i18n);

		// Assert
		expect(localizationStore.getLanguageLocaleOptions).toEqual([
			{
				text: 'English',
				code: 'en-US',
				iso: 'en-US',
				bcp47Code: 'en',
				img: '/img/flags/en-US.svg',
			},
			{
				text: 'Nederlands',
				code: 'nl-NL',
				iso: 'nl-NL',
				bcp47Code: 'nl',
				img: '/img/flags/nl-NL.svg',
			},
		]);
	});

	test('Should log an error and not throw when changing language before i18n is initialized', () => {
		// Arrange
		const localizationStore = useLocalizationStore();
		const logErrorSpy = vi.spyOn(Log, 'error').mockImplementation(() => undefined as never);

		// Act + Assert
		expect(() => localizationStore.changeLanguageLocale('en-US')).not.toThrow();
		expect(logErrorSpy).toHaveBeenCalledWith('i18n object is not defined');
	});
});
