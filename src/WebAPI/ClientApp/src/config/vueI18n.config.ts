import { defineI18nConfig } from '#i18n';

const defaultNumberFormat = {
	decimal: {
		style: 'decimal',
		minimumFractionDigits: 2,
		maximumFractionDigits: 2,
	},
	percent: {
		style: 'percent',
		useGrouping: false,
	},
} as const;

export default defineI18nConfig(() => ({
	locale: 'en-US',
	fallbackLocale: 'en-US',
	numberFormats: {
		'en-US': defaultNumberFormat,
		'de-DE': defaultNumberFormat,
		'fr-FR': defaultNumberFormat,
	},
}));
