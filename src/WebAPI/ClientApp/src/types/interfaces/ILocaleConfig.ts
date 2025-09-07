import type { Locale } from 'vue-i18n';

export interface ILocaleConfig {
	text: string;
	code: Locale;
	iso: string;
	bcp47Code: string;
	img: string;
}
